using ThingsEdge.Common.EventBus;
using ThingsEdge.Providers.Ops.Events;
using ThingsEdge.Providers.Ops.Snapshot;
using ThingsEdge.Router.Events;

namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 标记 <see cref="TagFlag.Heartbeat"/> 事件处理器。
/// </summary>
internal class HeartbeatHandler : INotificationHandler<HeartbeatEvent>
{
    private readonly IEventPublisher _publisher;
    private readonly ITagDataSnapshot _tagDataSnapshot;

    public HeartbeatHandler(IEventPublisher publisher, ITagDataSnapshot tagDataSnapshot)
    {
        _publisher = publisher;
        _tagDataSnapshot = tagDataSnapshot;
    }

    public async Task Handle(HeartbeatEvent notification, CancellationToken cancellationToken)
    {
        // 设置标记值快照。
        _tagDataSnapshot.Change(notification.Self);

        if (notification.IsOnlySign)
        {
            return;
        }

        var @event = DeviceHeartbeatEvent.Create(notification.ChannelName, notification.Device, notification.Tag, notification.IsConnected);
        await _publisher.Publish(@event, PublishStrategy.AsyncContinueOnException, cancellationToken).ConfigureAwait(false);
    }
}
