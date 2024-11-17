using System.Net;
using System.Net.Sockets;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Core.Pipe;

/// <summary>
/// 基于UDP/IT通信的管道信息。
/// </summary>
public class PipeUdpNet : PipeTcpNet
{
    /// <summary>
    /// 获取或设置一次接收时的数据长度，默认2KB数据长度，特殊情况的时候需要调整。
    /// </summary>
    public int ReceiveCacheLength { get; set; } = 2048;


    /// <summary>
    /// 实例化一个默认的对象。
    /// </summary>
    public PipeUdpNet()
    {
    }

    /// <summary>
    /// 通过指定的IP地址和端口号来实例化一个对象。
    /// </summary>
    /// <param name="ipAddress">IP地址信息</param>
    /// <param name="port">端口号</param>
    public PipeUdpNet(string ipAddress, int port)
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <inheritdoc />
    public override OperateResult<bool> OpenCommunication()
    {
        if (IsConnectError())
        {
            try
            {
                var connectIPEndPoint = GetConnectIPEndPoint();
                var socket = new Socket(connectIPEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                Socket = socket;
                ResetConnectErrorCount();
                return OperateResult.CreateSuccessResult(value: true);
            }
            catch (Exception ex)
            {
                CloseCommunication();
                return new OperateResult<bool>(-IncrConnectErrorCount(), ex.Message);
            }
        }
        return OperateResult.CreateSuccessResult(value: false);
    }

    /// <inheritdoc />
    public override async Task<OperateResult<bool>> OpenCommunicationAsync()
    {
        return await Task.Run(OpenCommunication).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <inheritdoc />
    public override OperateResult Send(byte[] data, int offset, int size)
    {
        var remoteEP = new IPEndPoint(IPAddress.Parse(IpAddress), Port);
        try
        {
            Socket.SendTo(data, offset, size, SocketFlags.None, remoteEP);
            return OperateResult.CreateSuccessResult();
        }
        catch (Exception ex)
        {
            CloseCommunication();
            return new OperateResult<byte[]>(-IncrConnectErrorCount(), ex.Message);
        }
    }

    /// <inheritdoc />
    public override async Task<OperateResult> SendAsync(byte[] data, int offset, int size)
    {
        return await Task.Run(() => Send(data, offset, size)).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <inheritdoc />
    public override OperateResult<int> Receive(byte[] buffer, int offset, int length, int timeOut = 60000, Action<long, long> reportProgress = null)
    {
        try
        {
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, ReceiveTimeOut);
            var iPEndPoint = new IPEndPoint(IPAddress.Parse(IpAddress), Port);
            var iPEndPoint2 = new IPEndPoint(iPEndPoint.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Any : IPAddress.Any, 0);
            EndPoint remoteEP = iPEndPoint2;
            if (length > 0)
            {
                var value = Socket.ReceiveFrom(buffer, offset, length, SocketFlags.None, ref remoteEP);
                return OperateResult.CreateSuccessResult(value);
            }
            var value2 = Socket.ReceiveFrom(buffer, offset, buffer.Length - offset, SocketFlags.None, ref remoteEP);
            return OperateResult.CreateSuccessResult(value2);
        }
        catch (Exception ex)
        {
            CloseCommunication();
            return new OperateResult<int>(-IncrConnectErrorCount(), "Socket Exception -> " + ex.Message);
        }
    }

    /// <inheritdoc />
    public override async Task<OperateResult<int>> ReceiveAsync(byte[] buffer, int offset, int length, int timeOut = 60000, Action<long, long> reportProgress = null)
    {
        return await Task.Run(() => Receive(buffer, offset, length, timeOut, reportProgress)).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <inheritdoc />
    public override OperateResult<byte[]> ReceiveMessage(INetMessage netMessage, byte[] sendValue, bool useActivePush = true, Action<long, long> reportProgress = null, Action<byte[]> logMessage = null)
    {
        if (UseServerActivePush)
        {
            return base.ReceiveMessage(netMessage, sendValue, useActivePush, reportProgress, logMessage);
        }
        return ReceiveMessage(netMessage, sendValue, null, logMessage);
    }

    /// <inheritdoc cref="M:HslCommunication.Core.Pipe.PipeUdpNet.ReceiveMessage(HslCommunication.Core.IMessage.INetMessage,System.Byte[],System.Boolean,System.Action{System.Int64,System.Int64},System.Action{System.Byte[]})" />
    /// <param name="alreadyReceive">已经接收的数据信息</param>
    public OperateResult<byte[]> ReceiveMessage(INetMessage netMessage, byte[] sendValue, byte[] alreadyReceive, Action<byte[]> logMessage = null, bool closeOnException = true)
    {
        try
        {
            var ms = new MemoryStream();
            if (alreadyReceive != null && alreadyReceive.Length != 0)
            {
                ms.Write(alreadyReceive);
                if (CheckMessageComplete(netMessage, sendValue, ref ms))
                {
                    return OperateResult.CreateSuccessResult(ms.ToArray());
                }
            }
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, ReceiveTimeOut);
            var iPEndPoint = new IPEndPoint(IPAddress.Parse(IpAddress), Port);
            var iPEndPoint2 = new IPEndPoint(iPEndPoint.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Any : IPAddress.Any, 0);
            EndPoint remoteEP = iPEndPoint2;
            var operateResult = NetSupport.CreateReceiveBuffer(ReceiveCacheLength);
            if (!operateResult.IsSuccess)
            {
                return operateResult;
            }
            var now = DateTime.Now;
            while (true)
            {
                var length = Socket.ReceiveFrom(operateResult.Content, ref remoteEP);
                var array = operateResult.Content.SelectBegin(length);
                ms.Write(array);
                logMessage?.Invoke(array);
                if (netMessage == null || CheckMessageComplete(netMessage, sendValue, ref ms))
                {
                    break;
                }
                if (ReceiveTimeOut >= 0 && (DateTime.Now - now).TotalMilliseconds > ReceiveTimeOut)
                {
                    return new OperateResult<byte[]>(StringResources.Language.ReceiveDataTimeout + ReceiveTimeOut + " Received: " + ms.ToArray().ToHexString(' '));
                }
            }
            return OperateResult.CreateSuccessResult(ms.ToArray());
        }
        catch (Exception ex)
        {
            if (closeOnException)
            {
                CloseCommunication();
            }
            return new OperateResult<byte[]>(-IncrConnectErrorCount(), "Socket Exception -> " + ex.Message);
        }
    }

    public override async Task<OperateResult<byte[]>> ReceiveMessageAsync(INetMessage netMessage, byte[] sendValue, bool useActivePush = true)
    {
        return await Task.Run(() => ReceiveMessage(netMessage, sendValue, useActivePush, reportProgress, logMessage)).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"PipeUdpNet[{IpAddress}:{Port}]";
    }
}
