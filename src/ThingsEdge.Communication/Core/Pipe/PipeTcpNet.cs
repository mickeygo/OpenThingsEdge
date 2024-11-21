using System.Net;
using System.Net.Sockets;
using ThingsEdge.Communication.Common;

namespace ThingsEdge.Communication.Core.Pipe;

/// <summary>
/// 用于TCP/IP协议的传输管道信息。
/// </summary>
public class PipeTcpNet : PipeNetBase
{
    private int[] _port = [2000];
    private int _indexPort = -1;

    /// <summary>
    /// 获取或是设置远程服务器的IP地址，如果是本机测试，那么需要设置为127.0.0.1。
    /// </summary>
    public string IpAddress { get; }

    public int SocketKeepAliveTime { get; set; } = -1;

    public int Port
    {
        get
        {
            if (_port.Length == 1)
            {
                return _port[0];
            }

            var num = _indexPort;
            if (num < 0 || num >= _port.Length)
            {
                num = 0;
            }
            return _port[num];
        }
        set
        {
            if (_port.Length == 1)
            {
                _port[0] = value;
                return;
            }
            var num = _indexPort;
            if (num < 0 || num >= _port.Length)
            {
                num = 0;
            }
            _port[num] = value;
        }
    }

    /// <summary>
    /// 获取或设置当前的客户端用于服务器连接的套接字。
    /// </summary>
    public Socket? Socket { get; protected set; }

    /// <summary>
    /// 连接超时时间，默认 10s。
    /// </summary>
    public int ConnectTimeOut { get; set; } = 10_000;

    public PipeTcpNet(string ipAddress, int port)
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <summary>
    /// 设置多个可选的端口号信息，例如在三菱的PLC里，支持配置多个端口号，当一个网络发生异常时，立即切换端口号连接读写，提升系统的稳定性。
    /// </summary>
    /// <param name="ports">端口号数组信息</param>
    public void SetMultiPorts(int[] ports)
    {
        if (ports != null && ports.Length != 0)
        {
            _port = ports;
            _indexPort = -1;
        }
    }

    /// <summary>
    /// 变更当前的端口号信息，如果设置了多个端口号的话，就切换其他可用的端口。
    /// </summary>
    private void ChangePorts()
    {
        if (_port.Length != 1)
        {
            if (_indexPort < _port.Length - 1)
            {
                _indexPort++;
            }
            else
            {
                _indexPort = 0;
            }
        }
    }

    /// <inheritdoc />
    public override bool IsConnectError()
    {
        if (Socket == null)
        {
            return true;
        }
        return base.IsConnectError();
    }

    /// <inheritdoc />
    public override async Task<OperateResult<bool>> OpenCommunicationAsync()
    {
        if (IsConnectError())
        {
            NetSupport.SafeCloseSocket(Socket);
            var endPoint = GetConnectIPEndPoint();
            var connect = await NetSupport.CreateSocketAndConnectAsync(endPoint, ConnectTimeOut).ConfigureAwait(false);
            if (connect.IsSuccess)
            {
                Debug.WriteLine("已成功创建 Socket 并连接上服务器");

                Socket = connect.Content;
                ResetConnectErrorCount();
                if (SocketKeepAliveTime > 0)
                {
                    Socket.SetKeepAlive(SocketKeepAliveTime, SocketKeepAliveTime);
                }
                return OperateResult.CreateSuccessResult(true);
            }
            return new OperateResult<bool>(-IncrConnectErrorCount(), connect.Message);
        }
        Debug.WriteLine("复用已有的 Socket");
        return OperateResult.CreateSuccessResult(false);
    }

    public override async Task<OperateResult> SendAsync(byte[] data)
    {
        var send = await NetSupport.SocketSendAsync(Socket, data).ConfigureAwait(false);
        if (!send.IsSuccess && send.ErrorCode == NetSupport.SocketErrorCode)
        {
            CloseCommunication();
            return new OperateResult<byte[]>(-IncrConnectErrorCount(), send.Message);
        }
        return send;
    }

    /// <inheritdoc />
    public override async Task<OperateResult<int>> ReceiveAsync(byte[] buffer, int offset, int length, int timeOut = 60000)
    {
        var receive = await NetSupport.SocketReceiveAsync(Socket, buffer, offset, length, timeOut).ConfigureAwait(false);
        if (!receive.IsSuccess && receive.ErrorCode == NetSupport.SocketErrorCode)
        {
            CloseCommunication();
            return new OperateResult<int>(-IncrConnectErrorCount(), "Socket Exception -> " + receive.Message);
        }
        return receive;
    }

    public override OperateResult CloseCommunication()
    {
        NetSupport.SafeCloseSocket(Socket);
        Socket = null;
        Debug.WriteLine("关闭并重置 Socket");

        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 获取当前的远程连接信息，如果端口号设置了可选的数组，那么每次获取对象就会发生端口号切换的操作。
    /// </summary>
    /// <returns>远程连接的对象</returns>
    private IPEndPoint GetConnectIPEndPoint()
    {
        if (_port.Length == 1)
        {
            return new IPEndPoint(IPAddress.Parse(IpAddress), _port[0]);
        }
        ChangePorts();
        var port = _port[_indexPort];
        return new IPEndPoint(IPAddress.Parse(IpAddress), port);
    }


    /// <inheritdoc />
    public override string ToString()
    {
        return $"PipeTcpNet[{IpAddress}:{Port}]";
    }
}
