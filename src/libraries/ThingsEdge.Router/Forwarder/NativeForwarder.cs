namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// Native 转发数据。
/// </summary>
internal sealed class NativeForwarder : IForwarder
{
    private readonly INativeForwarder _forwarder;

    public NativeForwarder(INativeForwarder forwarder)
    {
        _forwarder = forwarder;
    }

    public ForworderSource Source => ForworderSource.Native;

    public async Task<ResponseResult> SendAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _forwarder.SendAsync(message, cancellationToken);
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
