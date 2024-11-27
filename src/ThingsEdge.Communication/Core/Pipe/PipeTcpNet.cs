using System.Net;
using System.Net.Sockets;

namespace ThingsEdge.Communication.Core.Pipe;

/// <summary>
/// 用于TCP/IP协议的传输管道信息。
/// </summary>
public class PipeTcpNet(string ipAddress, int port) : NetworkPipeBase
{
    private Socket? _netSocket;

    /// <summary>
    /// 获取远程服务器的IP地址。
    /// </summary>
    public string IpAddress { get; } = ipAddress;

    /// <summary>
    /// 获取端口。
    /// </summary>
    public int Port { get; } = port;

    /// <summary>
    /// 连接超时时间，默认 5s。
    /// </summary>
    public int ConnectTimeout { get; set; } = 5_000;

    /// <summary>
    /// 获取或设置 Socket 保活时长，单位 ms。
    /// </summary>
    public int KeepAliveTime { get; set; } = -1;

    /// <summary>
    /// 是否为 Socket 的异常
    /// </summary>
    public bool IsSocketError { get; internal set; }

    /// <summary>
    /// Socket 在异常时关闭的委托对象，其中参数为错误状态码。
    /// </summary>
    public Action<int>? SocketErrorAndClosedDelegate { get; set; }

    public override async Task<OperateResult<bool>> CreateAndConnectPipeAsync()
    {
        var endPoint = new IPEndPoint(IPAddress.Parse(IpAddress), Port);
        var connect = await NetSupport.CreateSocketAndConnectAsync(endPoint, ConnectTimeout).ConfigureAwait(false);
        if (!connect.IsSuccess)
        {
            // 参考 NetSupport.CreateSocketAndConnectAsync 错误代码
            IsSocketError = connect.ErrorCode is (int)CommErrorCode.SocketConnectTimeoutException or (int)CommErrorCode.SocketConnectException;
            SocketErrorAndClosedDelegate?.Invoke(connect.ErrorCode);
            return new OperateResult<bool>(connect.ErrorCode, connect.Message);
        }

        Debug.WriteLine("已成功创建 Socket 并连接上服务器");

        _netSocket = connect.Content;
        if (KeepAliveTime > 0)
        {
            _netSocket.SetKeepAlive(KeepAliveTime, KeepAliveTime);
        }
        return OperateResult.CreateSuccessResult(true);
    }

    public override async Task<OperateResult> SendAsync(byte[] data)
    {
        if (_netSocket == null)
        {
            throw new UnconnectedException();
        }

        var result = await NetSupport.SocketSendAsync(_netSocket, data).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            // 参考 NetSupport.SocketSendAsync 错误代码
            IsSocketError = result.ErrorCode is (int)CommErrorCode.SocketSendException;
            SocketErrorAndClosedDelegate?.Invoke(result.ErrorCode);
        }

        return result;
    }

    public override async Task<OperateResult<int>> ReceiveAsync(byte[] buffer, int offset, int length, int timeout)
    {
        if (_netSocket == null)
        {
            throw new UnconnectedException();
        }

        var result = await NetSupport.SocketReceiveAsync(_netSocket, buffer, offset, length, timeout).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            // 参考 NetSupport.SocketReceiveAsync 错误代码
            IsSocketError = result.ErrorCode is (int)CommErrorCode.RemoteClosedConnection or (int)CommErrorCode.ReceiveDataTimeout or (int)CommErrorCode.SocketException;
            SocketErrorAndClosedDelegate?.Invoke(result.ErrorCode);
        }
        return result;
    }

    public override OperateResult ClosePipe()
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
