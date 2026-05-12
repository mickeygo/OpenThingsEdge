namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 心跳数据转发代理类。
/// </summary>
internal sealed class HeartbeatForwarderProxy(IServiceScopeFactory serviceScopeFactory) : IHeartbeatForwarderProxy
{
    public async Task ChangeAsync(HeartbeatContext context, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var forwarder = scope.ServiceProvider.GetService<IHeartbeatForwarder>();
        if (forwarder != null)
        {
            await forwarder.ReceiveAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }
}
