using ThingsEdge.Exchange.Contracts;

namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 请求数据发送处理代理类。
/// </summary>
internal sealed class RequestForwarderProxy(IServiceProvider serviceProvider) : IRequestForwarder
{
    public async Task<ResponseResult> SendAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            var forwarder = serviceProvider.GetRequiredService<IRequestForwarderHandler>();
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
