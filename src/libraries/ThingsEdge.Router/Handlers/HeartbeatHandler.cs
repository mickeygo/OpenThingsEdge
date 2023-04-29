using ThingsEdge.Router.Devices;
using ThingsEdge.Router.Events;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 心跳事件处理器。
/// </summary>
internal sealed class HeartbeatHandler : INotificationHandler<DeviceHeartbeatEvent>
{
    private readonly DeviceHealthMonitor _deviceHealthMonitor;

    public HeartbeatHandler(DeviceHealthMonitor deviceHealthMonitor)
    {
        _deviceHealthMonitor = deviceHealthMonitor;
    }

    public Task Handle(DeviceHeartbeatEvent notification, CancellationToken cancellationToken)
    {
        _deviceHealthMonitor.SetState(notification.Device.DeviceId, notification.ConnectState);
        return Task.CompletedTask;
    }
}
