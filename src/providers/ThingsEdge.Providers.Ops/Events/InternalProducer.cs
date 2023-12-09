namespace ThingsEdge.Providers.Ops.Events;

/// <summary>
/// 内部事件生产者。
/// </summary>
internal sealed class InternalProducer : IProducer, ISingletonDependency
{
    private readonly InternalEventBroker _broker;

    public InternalProducer(InternalEventBroker broker)
    {
        _broker = broker;
    }

    public async ValueTask ProduceAsync(IMonitorEvent item, CancellationToken cancellationToken = default)
    {
        await _broker.QueueAsync(item, cancellationToken).ConfigureAwait(false);
    }
}
