using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core.Pipe;

namespace ThingsEdge.Communication.Core.Net;

/// <summary>
/// 基于Udp的应答式通信类。
/// </summary>
public class NetworkUdpBase : NetworkBase
{
    /// <inheritdoc cref="F:HslCommunication.Core.Net.NetworkDoubleBase.LogMsgFormatBinary" />
    protected bool LogMsgFormatBinary = true;

    private int connectErrorCount = 0;

    private PipeSocket pipeSocket;

    /// <inheritdoc cref="P:HslCommunication.Core.Net.NetworkDoubleBase.IpAddress" />
    public virtual string IpAddress
    {
        get
        {
            return pipeSocket.IpAddress;
        }
        set
        {
            pipeSocket.IpAddress = value;
        }
    }

    /// <inheritdoc cref="P:HslCommunication.Core.Net.NetworkDoubleBase.Port" />
    public virtual int Port
    {
        get
        {
            return pipeSocket.Port;
        }
        set
        {
            pipeSocket.Port = value;
        }
    }

    /// <inheritdoc cref="P:HslCommunication.Core.Net.NetworkDoubleBase.ReceiveTimeOut" />
    public int ReceiveTimeout { get; set; }

    /// <inheritdoc cref="P:HslCommunication.Core.Net.NetworkDoubleBase.ConnectionId" />
    public string ConnectionId { get; set; }

    /// <summary>
    /// 获取或设置一次接收时的数据长度，默认2KB数据长度，特殊情况的时候需要调整<br />
    /// Gets or sets the length of data received at a time. The default length is 2KB
    /// </summary>
    public int ReceiveCacheLength { get; set; } = 2048;


    /// <inheritdoc cref="P:HslCommunication.Core.Net.NetworkDoubleBase.LocalBinding" />
    public IPEndPoint LocalBinding { get; set; }

    /// <summary>
    /// 实例化一个默认的方法<br />
    /// Instantiate a default method
    /// </summary>
    public NetworkUdpBase()
    {
        ReceiveTimeout = 5000;
        ConnectionId = SoftBasic.GetUniqueStringByGuidAndRandom();
        pipeSocket = new PipeSocket();
    }

    /// <inheritdoc cref="M:HslCommunication.Core.Net.NetworkDoubleBase.PackCommandWithHeader(System.Byte[])" />
    public virtual byte[] PackCommandWithHeader(byte[] command)
    {
        return command;
    }

    /// <inheritdoc cref="M:HslCommunication.Core.Net.NetworkDoubleBase.UnpackResponseContent(System.Byte[],System.Byte[])" />
    public virtual OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        return OperateResult.CreateSuccessResult(response);
    }

    /// <summary>
    /// 从UDP接收相关的数据信息，允许子类重写实现一些更加特殊的功能验证。
    /// </summary>
    /// <param name="socket">网络套接字</param>
    /// <param name="timeOut">超时时间</param>
    /// <param name="send">发送的报文信息</param>
    /// <returns>返回接收到的报文</returns>
    protected virtual byte[] ReceiveFromUdpSocket(Socket socket, int timeOut, byte[] send)
    {
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, ReceiveTimeout);
        var iPEndPoint = new IPEndPoint(IPAddress.Parse(IpAddress), Port);
        var iPEndPoint2 = new IPEndPoint(iPEndPoint.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Any : IPAddress.Any, 0);
        EndPoint remoteEP = iPEndPoint2;
        var array = new byte[ReceiveCacheLength];
        var length = socket.ReceiveFrom(array, ref remoteEP);
        return array.SelectBegin(length);
    }

    /// <summary>
    /// 核心的数据交互读取，发数据发送到通道上去，然后从通道上接收返回的数据<br />
    /// The core data is read interactively, the data is sent to the serial port, and the returned data is received from the serial port
    /// </summary>
    /// <param name="send">完整的报文内容</param>
    /// <param name="hasResponseData">是否有等待的数据返回，默认为 true</param>
    /// <param name="usePackAndUnpack">是否需要对命令重新打包，在重写<see cref="M:HslCommunication.Core.Net.NetworkUdpBase.PackCommandWithHeader(System.Byte[])" />方法后才会有影响</param>
    /// <returns>是否成功的结果对象</returns>
    public virtual OperateResult<byte[]> ReadFromCoreServer(byte[] send, bool hasResponseData, bool usePackAndUnpack)
    {
        var array = usePackAndUnpack ? PackCommandWithHeader(send) : send;
        Logger?.WriteDebug(ToString(), StringResources.Language.Send + " : " + (LogMsgFormatBinary ? SoftBasic.ByteToHexString(array) : Encoding.ASCII.GetString(array)));
        if (pipeSocket.LockingTick > CommunicationHelper.LockLimit)
        {
            return new OperateResult<byte[]>(StringResources.Language.TooManyLock);
        }
        pipeSocket.PipeLockEnter();
        try
        {
            var iPEndPoint = new IPEndPoint(IPAddress.Parse(IpAddress), Port);
            var availableSocketAsync = GetAvailableSocketAsync(iPEndPoint);
            if (!availableSocketAsync.IsSuccess)
            {
                pipeSocket.PipeLockLeave();
                return OperateResult.CreateFailedResult<byte[]>(availableSocketAsync);
            }
            availableSocketAsync.Content.SendTo(array, array.Length, SocketFlags.None, iPEndPoint);
            if (ReceiveTimeout < 0)
            {
                pipeSocket.PipeLockLeave();
                return OperateResult.CreateSuccessResult(new byte[0]);
            }
            if (!hasResponseData)
            {
                pipeSocket.PipeLockLeave();
                return OperateResult.CreateSuccessResult(new byte[0]);
            }
            var array2 = ReceiveFromUdpSocket(availableSocketAsync.Content, ReceiveTimeout, array);
            pipeSocket.PipeLockLeave();
            Logger?.WriteDebug(ToString(), StringResources.Language.Receive + " : " + (LogMsgFormatBinary ? SoftBasic.ByteToHexString(array2) : Encoding.ASCII.GetString(array2)));
            connectErrorCount = 0;
            pipeSocket.IsSocketError = false;
            try
            {
                return usePackAndUnpack ? UnpackResponseContent(array, array2) : OperateResult.CreateSuccessResult(array2);
            }
            catch (Exception ex)
            {
                return new OperateResult<byte[]>("UnpackResponseContent failed: " + ex.Message);
            }
        }
        catch (Exception ex2)
        {
            pipeSocket.ChangePorts();
            pipeSocket.IsSocketError = true;
            if (connectErrorCount < 100000000)
            {
                connectErrorCount++;
            }
            pipeSocket.PipeLockLeave();
            return new OperateResult<byte[]>(-connectErrorCount, ex2.Message);
        }
    }

    /// <inheritdoc cref="M:HslCommunication.Core.Net.NetworkUdpBase.ReadFromCoreServer(System.Byte[],System.Boolean,System.Boolean)" />
    public async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(byte[] value)
    {
        return await Task.Run(() => ReadFromCoreServer(value));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.Net.NetworkUdpBase.ReadFromCoreServer(System.Collections.Generic.IEnumerable{System.Byte[]})" />
    public async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(IEnumerable<byte[]> send)
    {
        return await NetSupport.ReadFromCoreServerAsync(send, ReadFromCoreServerAsync);
    }

    /// <inheritdoc cref="M:HslCommunication.Core.Net.NetworkDoubleBase.IpAddressPing" />
    public IPStatus IpAddressPing()
    {
        var ping = new Ping();
        return ping.Send(IpAddress).Status;
    }

    /// <inheritdoc cref="M:HslCommunication.Core.Net.NetworkDoubleBase.SetPipeSocket(HslCommunication.Core.Pipe.PipeSocket)" />
    public void SetPipeSocket(PipeSocket pipeSocket)
    {
        if (this.pipeSocket != null)
        {
            this.pipeSocket = pipeSocket;
        }
    }

    /// <inheritdoc cref="M:HslCommunication.Core.Net.NetworkDoubleBase.GetPipeSocket" />
    public PipeSocket GetPipeSocket()
    {
        return pipeSocket;
    }

    private OperateResult<Socket> GetAvailableSocketAsync(IPEndPoint endPoint)
    {
        if (pipeSocket.IsConnectitonError())
        {
            OperateResult operateResult = null;
            try
            {
                pipeSocket.Socket?.Close();
                var socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                if (LocalBinding != null)
                {
                    socket.Bind(LocalBinding);
                }
                pipeSocket.Socket = socket;
                operateResult = OperateResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                pipeSocket.IsSocketError = true;
                operateResult = new OperateResult(ex.Message);
            }
            if (!operateResult.IsSuccess)
            {
                pipeSocket.IsSocketError = true;
                return OperateResult.CreateFailedResult<Socket>(operateResult);
            }
            pipeSocket.IsSocketError = false;
            return OperateResult.CreateSuccessResult(pipeSocket.Socket);
        }
        return OperateResult.CreateSuccessResult(pipeSocket.Socket);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"NetworkUdpBase[{IpAddress}:{Port}]";
    }
}
