namespace ThingsEdge.Communication.Core.Pipe;

/// <summary>
/// PipeTcpNet 的副本，用于连接池访问。
/// </summary>
internal sealed class PipeTcpNetCopy(PipeTcpNet pipeTcpNet, SocketWrapper activeSocket) : NetworkPipeBase
{
    public override async Task<OperateResult> SendAsync(byte[] data)
    {
        var result = await NetSupport.SocketSendAsync(activeSocket, data).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            // 参考 NetSupport.SocketSendAsync 错误代码
            pipeTcpNet.IsSocketError = result.ErrorCode is (int)CommErrorCode.SocketSendException;
            pipeTcpNet.SocketErrorAndClosedDelegate?.Invoke(result.ErrorCode);
        }

        return result;
    }

    public override async Task<OperateResult<int>> ReceiveAsync(byte[] buffer, int offset, int length, int timeout)
    {
        var result = await NetSupport.SocketReceiveAsync(activeSocket, buffer, offset, length, timeout).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            // 参考 NetSupport.SocketReceiveAsync 错误代码
            pipeTcpNet.IsSocketError = result.ErrorCode is (int)CommErrorCode.RemoteClosedConnection or (int)CommErrorCode.ReceiveDataTimeout or (int)CommErrorCode.SocketException;
            pipeTcpNet.SocketErrorAndClosedDelegate?.Invoke(result.ErrorCode);
        }
        return result;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // 回收 Socket
            pipeTcpNet.ReleaseConnection(activeSocket);
        }
    }
}
