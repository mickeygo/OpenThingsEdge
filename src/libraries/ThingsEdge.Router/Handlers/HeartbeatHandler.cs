using ThingsEdge.Router.Devices;
using ThingsEdge.Router.Events;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 心跳事件处理器。
/// </summary>
internal sealed class HeartbeatHandler : INotificationHandler<HeartbeatEvent>
{
    private readonly DeviceHealthMonitor _deviceHealthMonitor;

    public HeartbeatHandler(DeviceHealthMonitor deviceHealthMonitor)
    {
        _deviceHealthMonitor = deviceHealthMonitor;
    }

    public Task Handle(HeartbeatEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
