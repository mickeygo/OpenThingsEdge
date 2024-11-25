namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 通知数据转发代理类。
/// </summary>
internal sealed class NoticeForwarderProxy(IServiceProvider serviceProvider) : INoticeForwarderProxy
{
    public async Task PublishAsync(NoticeContext context, CancellationToken cancellationToken = default)
    {
        var forwarder = serviceProvider.GetService<INoticeForwarder>();
        if (forwarder != null)
        {
            await forwarder.PublishAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }
}
