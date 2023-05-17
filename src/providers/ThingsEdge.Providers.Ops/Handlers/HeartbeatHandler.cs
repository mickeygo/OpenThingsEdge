using ThingsEdge.Common.EventBus;
using ThingsEdge.Router.Events;

namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 标记 <see cref="TagFlag.Heartbeat"/> 事件处理器。
/// </summary>
internal class HeartbeatHandler : INotificationHandler<HeartbeatEvent>
{
    private readonly IEventPublisher _publisher;

    public HeartbeatHandler(IEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task Handle(HeartbeatEvent notification, CancellationToken cancellationToken)
    {
        var @event = DeviceHeartbeatEvent.Create(notification.ChannelName, notification.Device, notification.Tag, notification.IsConnected);
        await _publisher.Publish(@event, PublishStrategy.ParallelNoWait, cancellationToken).ConfigureAwait(false);
    }
}
