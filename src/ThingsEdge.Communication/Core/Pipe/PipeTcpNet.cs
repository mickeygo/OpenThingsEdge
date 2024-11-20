using System.Net;
using System.Net.Sockets;
using ThingsEdge.Communication.Common;

namespace ThingsEdge.Communication.Core.Pipe;

/// <summary>
/// 用于TCP/IP协议的传输管道信息。
/// </summary>
public class PipeTcpNet : PipeNetBase
{
    private string _ipAddress = "127.0.0.1";

    private int[] _port = [2000];

    private int _indexPort = -1;

    /// <summary>
    /// 获取或是设置远程服务器的IP地址，如果是本机测试，那么需要设置为127.0.0.1。
    /// </summary>
    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            Host = value;
            _ipAddress = CommunicationHelper.GetIpAddressFromInput(value);
        }
    }

    /// <summary>
    /// 获取当前设置的远程的地址，可能是IP地址，也可能是网址，也就是初始设置的地址信息。
    /// </summary>
    public string Host { get; private set; } = "127.0.0.1";

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

    /// <summary>
    /// 当管道打开成功的时候执行的事件，如果返回失败，则管道的打开操作返回失败。
    /// </summary>
    /// <param name="socket">通信的套接字</param>
    /// <returns>是否真的打开成功</returns>
    protected virtual OperateResult OnCommunicationOpen(Socket socket)
    {
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc />
    public override async Task<OperateResult<bool>> OpenCommunicationAsync()
    {
        if (IsConnectError())
        {
            NetSupport.CloseSocket(Socket);
            var endPoint = GetConnectIPEndPoint();
            var connect = await NetSupport.CreateSocketAndConnectAsync(endPoint, ConnectTimeOut).ConfigureAwait(false);
            if (connect.IsSuccess)
            {
                var onOpen = OnCommunicationOpen(connect.Content);
                if (!onOpen.IsSuccess)
                {
                    return onOpen.ConvertFailed<bool>();
                }

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
        return OperateResult.CreateSuccessResult(false);
    }

    public override async Task<OperateResult> SendAsync(byte[] data)
    {
        var send = await NetSupport.SocketSendAsync(Socket, data).ConfigureAwait(false);
        if (!send.IsSuccess && send.ErrorCode == NetSupport.SocketErrorCode)
        {
            await CloseCommunicationAsync().ConfigureAwait(false);
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
            await CloseCommunicationAsync().ConfigureAwait(false);
            return new OperateResult<int>(-IncrConnectErrorCount(), "Socket Exception -> " + receive.Message);
        }
        return receive;
    }

    /// <inheritdoc />
    public override async Task<OperateResult> CloseCommunicationAsync()
    {
        NetSupport.CloseSocket(Socket);
        Socket = null;
        return await Task.FromResult(OperateResult.CreateSuccessResult()).ConfigureAwait(false);
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
        return $"PipeTcpNet[{_ipAddress}:{Port}]";
    }
}
