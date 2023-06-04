namespace ThingsEdge.Providers.Ops.Events;

internal sealed class EventLoop : ISingletonDependency
{
    private readonly InternalEventBroker _broker;
    private readonly EventDispatcher _dispatcher;

    public EventLoop(InternalEventBroker broker, EventDispatcher dispatcher)
    {
        _broker = broker;
        _dispatcher = dispatcher;
    }

    public async Task Loop(CancellationToken cancellationToken = default)
    {
        while (await _broker.WaitToReadAsync().ConfigureAwait(false))
        {
            while (_broker.TryRead(out var @event))
            {
                await _dispatcher.DispatchAsync(@event).ConfigureAwait(false);
            }
        }
    }
}
