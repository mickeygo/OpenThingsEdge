using ThingsEdge.Exchange.Contracts;

namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 触发数据转发代理接口。
/// </summary>
internal sealed class TriggerForwarderProxy(IServiceProvider serviceProvider) : ITriggerForwarderProxy
{
    public async Task<ResponseResult> SendAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        var forwarder = serviceProvider.GetService<ITriggerForwarder>();
        if (forwarder == null)
        {
            return ResponseResult.FromError(ExchangeErrorCode.ForwarderUnregister, "请求服务处理接口 [ITriggerForwarder] 还未注册");
        }

        try
        {
            var resp = await forwarder.HandleAsync(message, cancellationToken).ConfigureAwait(false);
            return ResponseResult.FromOk(resp);
        }
        catch (OperationCanceledException ex)
        {
            return ResponseResult.FromError(ExchangeErrorCode.HandleRequestTimedOut, $"数据请求处理超时，错误：{ex.Message}");
        }
        catch (Exception ex)
        {
            return ResponseResult.FromError(ExchangeErrorCode.HandleError, $"数据处理服务异常，错误：{ex.Message}");
        }
    }
}
