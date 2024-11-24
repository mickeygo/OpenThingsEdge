namespace ThingsEdge.Exchange.Events;

/// <summary>
/// 事件循环处理器。
/// </summary>
internal sealed class EventLoop
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
        while (!cancellationToken.IsCancellationRequested)
        {
            var @event = await _broker.DequeueAsync(cancellationToken).ConfigureAwait(false);
            await _dispatcher.DispatchAsync(@event).ConfigureAwait(false);
        }
    }
}
