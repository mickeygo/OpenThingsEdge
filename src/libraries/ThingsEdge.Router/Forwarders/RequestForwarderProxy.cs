namespace ThingsEdge.Router.Forwarders;

/// <summary>
/// 请求数据发送处理代理类。
/// </summary>
internal sealed class RequestForwarderProxy(IServiceScopeFactory serviceScopeFactory) : IRequestForwarder
{
    public async Task<ResponseResult> SendAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var forwarder = scope.ServiceProvider.GetRequiredService<IRequestForwarderHandler>();
            var resp = await forwarder.HandleAsync(message, cancellationToken);
            return ResponseResult.FromOk(resp);
        }
        catch (OperationCanceledException ex)
        {
            return ResponseResult.FromError(ErrorCode.HandleRequestTimedOut, $"数据请求处理超时，错误：{ex.Message}");
        }
        catch (Exception ex)
        {
            return ResponseResult.FromError(ErrorCode.HandleError, $"数据处理服务异常，错误：{ex.Message}");
        }
    }
}
