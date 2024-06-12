using ThingsEdge.Providers.Ops.Events;
using ThingsEdge.Providers.Ops.Exchange;
using ThingsEdge.Providers.Ops.Snapshot;

namespace ThingsEdge.Providers.Ops.Handlers;

internal sealed class ExchangeChangedHandler(ITagDataSnapshot tagDataSnapshot) : INotificationHandler<ExchangeChangedEvent>
{
    public Task Handle(ExchangeChangedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.State == RunningState.Stop)
        {
            tagDataSnapshot.Clear();
            TagValueSet.Clear();
        }

        return Task.CompletedTask;
    }
}
