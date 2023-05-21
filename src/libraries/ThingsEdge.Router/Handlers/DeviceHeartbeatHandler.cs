using ThingsEdge.Router.Events;
using ThingsEdge.Router.Interfaces;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 设备心跳事件处理器。
/// </summary>
internal sealed class DeviceHeartbeatHandler : INotificationHandler<DeviceHeartbeatEvent>
{
    private readonly IServiceProvider _serviceProvider;

    public DeviceHeartbeatHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Handle(DeviceHeartbeatEvent notification, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var devHeartbeatApi = scope.ServiceProvider.GetService<IDeviceHeartbeatApi>();
        if (devHeartbeatApi != null)
        {
            await devHeartbeatApi.ChangeAsync(notification.ChannelName, notification.Device, notification.Tag, notification.IsOnline, cancellationToken).ConfigureAwait(false);
        }
    }
}
