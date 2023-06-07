using ThingsEdge.Router.Events;

namespace ThingsEdge.Providers.Ops.Events;

internal sealed class InternalProducer : IProducer, ISingletonDependency
{
    private readonly InternalEventBroker _broker;

    public InternalProducer(InternalEventBroker broker)
    {
        _broker = broker;
    }

    public async ValueTask ProduceAsync(IEvent item, CancellationToken cancellationToken = default)
    {
        await _broker.QueueAsync(item, cancellationToken).ConfigureAwait(false);
    }
}
