namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 开关数据转发代理接口。
/// </summary>
internal sealed class SwitchForwarderProxy(IServiceScopeFactory serviceScopeFactory) : ISwitchForwarderProxy
{
    public async Task PublishAsync(SwitchContext context, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var forwarder = scope.ServiceProvider.GetService<ISwitchForwarder>();
        if (forwarder != null)
        {
            await forwarder.ReceiveAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }
}
