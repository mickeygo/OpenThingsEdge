using ThingsEdge.Router.Model;

namespace ThingsEdge.Router.Devices;

/// <summary>
/// 设备健康状态监控器。
/// </summary>
public sealed class DeviceHealthMonitor
{
    private readonly ConcurrentDictionary<string, DeviceHealthState> _map = new();

    internal void SetState(string deviceId, DeviceConnectState connectState)
    {
        _map.AddOrUpdate(deviceId, new DeviceHealthState { DeviceId = deviceId, ConnectState = connectState }, (_, state) =>
        {
            state.ConnectState = connectState;
            return state;
        });
    }
}

public sealed class DeviceHealthState
{
    /// <summary>
    /// 设备 Id。
    /// </summary>
    [NotNull]
    public string? DeviceId { get; init; }

    /// <summary>
    /// 设备连接状态。
    /// </summary>
    public DeviceConnectState ConnectState { get; set; }
}
