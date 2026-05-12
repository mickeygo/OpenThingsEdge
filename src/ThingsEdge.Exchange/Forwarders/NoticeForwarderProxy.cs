namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 通知数据转发代理类。
/// </summary>
internal sealed class NoticeForwarderProxy(IServiceScopeFactory serviceScopeFactory) : INoticeForwarderProxy
{
    public async Task PublishAsync(NoticeContext context, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var forwarder = scope.ServiceProvider.GetService<INoticeForwarder>();
        if (forwarder != null)
        {
            await forwarder.ReceiveAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }
}
