namespace ThingsEdge.Providers.Ops.Events;

/// <summary>
/// 内部事件生产者。
/// </summary>
internal sealed class InternalProducer(InternalEventBroker broker) : IProducer, ISingletonDependency
{
    public async ValueTask ProduceAsync(IMonitorEvent item, CancellationToken cancellationToken = default)
    {
        await broker.QueueAsync(item, cancellationToken).ConfigureAwait(false);
    }
}
