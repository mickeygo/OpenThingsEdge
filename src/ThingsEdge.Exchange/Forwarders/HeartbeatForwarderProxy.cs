namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 心跳数据转发代理类。
/// </summary>
/// <param name="serviceProvider"></param>
internal sealed class HeartbeatForwarderProxy(IServiceProvider serviceProvider) : IHeartbeatForwarderProxy
{
    public async Task ChangeAsync(HeartbeatContext context, CancellationToken cancellationToken)
    {
        var forwarder = serviceProvider.GetService<IHeartbeatForwarder>();
        if (forwarder != null)
        {
            await forwarder.ChangeAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }
}
