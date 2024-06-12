using ThingsEdge.Router.Events;
using ThingsEdge.Router.Interfaces;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 设备心跳事件处理器。
/// </summary>
internal sealed class DeviceHeartbeatHandler(IServiceScopeFactory serviceScopeFactory) : INotificationHandler<DeviceHeartbeatEvent>
{
    public async Task Handle(DeviceHeartbeatEvent notification, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var devHeartbeatApi = scope.ServiceProvider.GetService<IDeviceHeartbeatApi>();
        if (devHeartbeatApi != null)
        {
            await devHeartbeatApi.ChangeAsync(notification.ChannelName, notification.Device, notification.Tag, notification.IsOnline, cancellationToken).ConfigureAwait(false);
        }
    }
}
