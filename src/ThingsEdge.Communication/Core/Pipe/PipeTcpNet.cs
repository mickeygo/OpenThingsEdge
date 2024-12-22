using System.Net.Sockets;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core.ConnectionPool;
using ThingsEdge.Communication.Core.Device;

namespace ThingsEdge.Communication.Core.Pipe;

/// <summary>
/// 用于TCP/IP协议的传输管道信息。
/// </summary>
public class PipeTcpNet : NetworkPipeBase
{
    private readonly SocketPool _socketPool;

    /// <summary>
    /// 获取远程服务器的IP地址。
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// 获取端口。
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// 连接超时时间，单位 ms。
    /// </summary>
    public int ConnectTimeout { get; }

    /// <summary>
    /// 是否为 Socket 的异常
    /// </summary>
    public bool IsSocketError { get; internal set; }

    /// <summary>
    /// Socket 在异常时关闭的委托对象，其中参数为错误状态码。
    /// </summary>
    public Action<int>? SocketErrorAndClosedDelegate { get; set; }

    /// <summary>
    /// 初始 <see cref="PipeTcpNet"/> 对象。
    /// </summary>
    /// <param name="host">服务IP地址</param>
    /// <param name="port">端口</param>
    /// <param name="options">其他选项</param>
    public PipeTcpNet(string host, int port, DeviceTcpNetOptions options)
    {
        Host = host;
        Port = port;
        ConnectTimeout = options.ConnectTimeout;
        _socketPool = new(Host, Port, Math.Max(1, options.SocketPoolSize))
        {
            ConnectTimeout = TimeSpan.FromMilliseconds(Math.Max(3_000, options.ConnectTimeout)),
            KeepAliveTime = options.KeepAliveTime,
        };
    }

    public override async Task<OperateResult<bool>> CreateAndConnectPipeAsync()
    {
        try
        {
            await _socketPool.GetAndReturnAsync().ConfigureAwait(false);
            Debug.WriteLine("连接服务器成功");

            return OperateResult.CreateSuccessResult(true);
        }
        catch (OperationCanceledException)
        {
            IsSocketError = true;
            var errorCode1 = (int)CommErrorCode.SocketConnectTimeoutException;
            SocketErrorAndClosedDelegate?.Invoke(errorCode1);
            return new OperateResult<bool>(errorCode1, string.Format(StringResources.Language.ConnectTimeout, $"{Host}:{Port}", ConnectTimeout) + " ms");
        }
        catch (SocketException ex)
        {
            IsSocketError = true;
            var errorCode2 = (int)CommErrorCode.SocketConnectException;
            SocketErrorAndClosedDelegate?.Invoke(errorCode2);
            return new OperateResult<bool>(errorCode2, $"Socket Connect Exception -> {ex.Message}");
        }
    }

    public override async Task<OperateResult> SendAsync(byte[] data)
    {
        return await _socketPool.DoAndReturnAsync(async (socket) =>
        {
            var result = await NetSupport.SocketSendAsync(socket, data).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                // 参考 NetSupport.SocketSendAsync 错误代码
                IsSocketError = result.ErrorCode is (int)CommErrorCode.SocketSendException;
                SocketErrorAndClosedDelegate?.Invoke(result.ErrorCode);
            }

            return result;
        }).ConfigureAwait(false);
    }

    public override async Task<OperateResult<int>> ReceiveAsync(byte[] buffer, int offset, int length, int timeout)
    {
        return await _socketPool.DoAndReturnAsync(async (socket) =>
        {
            var result = await NetSupport.SocketReceiveAsync(socket, buffer, offset, length, timeout).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                // 参考 NetSupport.SocketReceiveAsync 错误代码
                IsSocketError = result.ErrorCode is (int)CommErrorCode.RemoteClosedConnection or (int)CommErrorCode.ReceiveDataTimeout or (int)CommErrorCode.SocketException;
                SocketErrorAndClosedDelegate?.Invoke(result.ErrorCode);
            }
            return result;
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// 创建一个副本
    /// </summary>
    internal async Task<OperateResult<NetworkPipeBase>> CreateCopyAsync()
    {
        try
        {
            var socket = await _socketPool.GetConnectionAsync().ConfigureAwait(false);
            NetworkPipeBase pipe2 = new PipeTcpNetCopy(this, socket);
            return OperateResult.CreateSuccessResult(pipe2);
        }
        catch (OperationCanceledException)
        {
            var errorCode1 = (int)CommErrorCode.SocketConnectTimeoutException;
            SocketErrorAndClosedDelegate?.Invoke(errorCode1);
            return new OperateResult<NetworkPipeBase>(errorCode1, string.Format(StringResources.Language.ConnectTimeout, $"{Host}:{Port}", ConnectTimeout) + " ms");
        }
        catch (SocketException ex)
        {
            var errorCode2 = (int)CommErrorCode.SocketConnectException;
            SocketErrorAndClosedDelegate?.Invoke(errorCode2);
            return new OperateResult<NetworkPipeBase>(errorCode2, $"Socket Connect Exception -> {ex.Message}");
        }
    }

    /// <summary>
    /// 线程池回收连接。
    /// </summary>
    /// <param name="socket"></param>
    internal void ReleaseConnection(SocketWrapper socket)
    {
        _socketPool.ReleaseConnection(socket);
    }

    public override OperateResult ClosePipe()
    {
        Debug.WriteLine("关闭并重置 Socket");
        _socketPool.Dispose();

        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"PipeTcpNet[{Host}:{Port}]";
    }
}
