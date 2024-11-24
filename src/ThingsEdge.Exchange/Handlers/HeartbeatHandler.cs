using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Engine.Snapshot;
using ThingsEdge.Exchange.Events;
using ThingsEdge.Exchange.Infrastructure.EventBus;

namespace ThingsEdge.Exchange.Handlers;

/// <summary>
/// 标记 <see cref="TagFlag.Heartbeat"/> 事件处理器。
/// </summary>
internal sealed class HeartbeatHandler(IEventPublisher publisher, ITagDataSnapshot tagDataSnapshot) : INotificationHandler<HeartbeatEvent>
{
    public async Task Handle(HeartbeatEvent notification, CancellationToken cancellationToken)
    {
        // 设置标记值快照。
        tagDataSnapshot.Change(notification.Self);

        // 若是信号值，则中断处理。
        if (notification.IsOnlySign)
        {
            return;
        }

        var @event = new DeviceHeartbeatEvent(notification.ChannelName, notification.Device, notification.Tag, notification.IsConnected);
        await publisher.Publish(@event, PublishStrategy.AsyncContinueOnException, cancellationToken).ConfigureAwait(false);
    }
}
