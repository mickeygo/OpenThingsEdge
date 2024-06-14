using ThingsEdge.Router.Events;
using ThingsEdge.Router.Forwarders;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 设备心跳事件处理器。
/// </summary>
internal sealed class DeviceHeartbeatHandler(IServiceProvider serviceProvider) : INotificationHandler<DeviceHeartbeatEvent>
{
    public async Task Handle(DeviceHeartbeatEvent notification, CancellationToken cancellationToken)
    {
        var forwarder = serviceProvider.GetService<INativeHeartbeatForwarder>();
        if (forwarder != null)
        {
            await forwarder.ChangeAsync(notification.ChannelName, notification.Device, notification.Tag, notification.IsOnline, cancellationToken).ConfigureAwait(false);
        }
    }
}
