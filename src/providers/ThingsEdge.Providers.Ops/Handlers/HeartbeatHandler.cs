using ThingsEdge.Common.EventBus;
using ThingsEdge.Providers.Ops.Events;
using ThingsEdge.Providers.Ops.Snapshot;
using ThingsEdge.Router.Events;

namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 标记 <see cref="TagFlag.Heartbeat"/> 事件处理器。
/// </summary>
internal sealed class HeartbeatHandler(IEventPublisher publisher, ITagDataSnapshot tagDataSnapshot) : INotificationHandler<HeartbeatEvent>
{
    public async Task Handle(HeartbeatEvent notification, CancellationToken cancellationToken)
    {
        // 设置标记值快照。
        tagDataSnapshot.Change(notification.Self);

        if (notification.IsOnlySign)
        {
            return;
        }

        var @event = new DeviceHeartbeatEvent(notification.ChannelName, notification.Device, notification.Tag, notification.IsConnected);
        await publisher.Publish(@event, PublishStrategy.AsyncContinueOnException, cancellationToken).ConfigureAwait(false);
    }
}
