using System.Net;
using System.Net.Sockets;

namespace ThingsEdge.Communication.Core.Pipe;

/// <summary>
/// 用于TCP/IP协议的传输管道信息。
/// </summary>
public class PipeTcpNet : NetworkPipeBase
{
    private Socket? _netSocket;

    /// <summary>
    /// 获取远程服务器的IP地址。
    /// </summary>
    public string IpAddress { get; }

    /// <summary>
    /// 获取端口。
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// 连接超时时间，默认 10s。
    /// </summary>
    public int ConnectTimeout { get; set; } = 10_000;

    /// <summary>
    /// 获取或设置 Socket 保活时长，单位 ms。
    /// </summary>
    public int KeepAliveTime { get; set; } = -1;

    public PipeTcpNet(string ipAddress, int port)
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <inheritdoc />
    public override bool IsConnectError()
    {
        if (_netSocket == null)
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
            _netSocket.SafeClose();
            var endPoint = new IPEndPoint(IPAddress.Parse(IpAddress), Port);
            var connect = await NetSupport.CreateSocketAndConnectAsync(endPoint, ConnectTimeout).ConfigureAwait(false);
            if (connect.IsSuccess)
            {
                Debug.WriteLine("已成功创建 Socket 并连接上服务器");

                _netSocket = connect.Content;
                ResetConnectErrorCount();
                if (KeepAliveTime > 0)
                {
                    _netSocket.SetKeepAlive(KeepAliveTime, KeepAliveTime);
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
        if (_netSocket == null)
        {
            throw new NullReferenceException();
        }

        var send = await NetSupport.SocketSendAsync(_netSocket, data).ConfigureAwait(false);
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
        if (_netSocket == null)
        {
            throw new NullReferenceException();
        }

        var receive = await NetSupport.SocketReceiveAsync(_netSocket, buffer, offset, length, timeOut).ConfigureAwait(false);
        if (!receive.IsSuccess && receive.ErrorCode == NetSupport.SocketErrorCode)
        {
            CloseCommunication();
            return new OperateResult<int>(-IncrConnectErrorCount(), "Socket Exception -> " + receive.Message);
        }
        return receive;
    }

    public override OperateResult CloseCommunication()
    {
        _netSocket.SafeClose();
        _netSocket = null;
        Debug.WriteLine("关闭并重置 Socket");

        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"PipeTcpNet[{IpAddress}:{Port}]";
    }
}
