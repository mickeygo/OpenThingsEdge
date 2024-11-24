using ThingsEdge.Exchange.Engine;
using ThingsEdge.Exchange.Engine.Snapshot;
using ThingsEdge.Exchange.Events;

namespace ThingsEdge.Exchange.Handlers;

internal sealed class ExchangeChangedHandler(ITagDataSnapshot tagDataSnapshot) : INotificationHandler<ExchangeChangedEvent>
{
    public Task Handle(ExchangeChangedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.State == RunningState.Stop)
        {
            tagDataSnapshot.Clear();
            TagDataCache.Clear();
        }

        return Task.CompletedTask;
    }
}
