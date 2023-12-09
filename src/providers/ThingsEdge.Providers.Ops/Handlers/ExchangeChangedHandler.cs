using ThingsEdge.Providers.Ops.Events;
using ThingsEdge.Providers.Ops.Exchange;
using ThingsEdge.Providers.Ops.Snapshot;

namespace ThingsEdge.Providers.Ops.Handlers;

internal sealed class ExchangeChangedHandler : INotificationHandler<ExchangeChangedEvent>
{
    private readonly ITagDataSnapshot _tagDataSnapshot;

    public ExchangeChangedHandler(ITagDataSnapshot tagDataSnapshot)
    {
        _tagDataSnapshot = tagDataSnapshot;
    }

    public Task Handle(ExchangeChangedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.State == RunningState.Stop)
        {
            _tagDataSnapshot.Clear();
            TagValueSet.Clear();
        }

        return Task.CompletedTask;
    }
}
