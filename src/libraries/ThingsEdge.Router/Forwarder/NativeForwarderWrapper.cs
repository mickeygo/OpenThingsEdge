namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// Native 转发数据包装类。
/// </summary>
internal sealed class NativeForwarderWrapper(INativeForwarder forwarder) : IForwarder
{
    public ForworderSource Source => ForworderSource.Native;

    public async Task<ResponseResult> SendAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await forwarder.SendAsync(message, cancellationToken);
            return ResponseResult.FromOk(response, Source);
        }
        catch (OperationCanceledException ex)
        {
            return ResponseResult.FromError(ErrorCode.NativeRequestTimedOut, $"Native 请求服务超时，错误：{ex.Message}", Source);
        }
        catch (Exception ex)
        {
            return ResponseResult.FromError(ErrorCode.NativeError, $"Native 服务异常，错误：{ex.Message}", Source);
        }
    }
}
