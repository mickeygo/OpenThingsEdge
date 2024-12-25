namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 开关数据转发代理接口。
/// </summary>
internal sealed class SwitchForwarderProxy(IServiceProvider serviceProvider) : ISwitchForwarderProxy
{
    public async Task PublishAsync(SwitchContext context, CancellationToken cancellationToken = default)
    {
        var forwarder = serviceProvider.GetService<ISwitchForwarder>();
        if (forwarder != null)
        {
            await forwarder.PublishAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }
}
