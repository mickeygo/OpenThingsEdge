using ThingsEdge.Router.Model;

namespace ThingsEdge.Router.Devices;

/// <summary>
/// 设备健康状态监控器。
/// </summary>
public sealed class DeviceHealthMonitor
{
    private readonly ConcurrentDictionary<string, DeviceHealthState> _map = new();

    /// <summary>
    /// 获取设备连接状态，没有找到设备则返回 null。
    /// </summary>
    /// <param name="deviceId">设备 Id。</param>
    /// <returns></returns>
    public DeviceHealthState? Get(string deviceId)
    {
        _map.TryGetValue(deviceId, out var state);
        return state;
    }

    /// <summary>
    /// 设置设备连接状态。
    /// </summary>
    /// <param name="deviceId">设备 Id。</param>
    /// <param name="connectState">设备状态。</param>
    internal void SetState(string deviceId, DeviceConnectState connectState)
    {
        _map.AddOrUpdate(deviceId, new DeviceHealthState { DeviceId = deviceId, ConnectState = connectState }, (_, state) =>
        {
            if (state.ConnectState != connectState)
            {
                state.ConnectState = connectState;
                state.UpdatedTime = DateTime.Now;
            }

            return state;
        });
    }
}

/// <summary>
/// 设备健康状态。
/// </summary>
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
    public DeviceConnectState ConnectState { get; internal set; }

    /// <summary>
    /// 上一次连接状态更改时间。
    /// </summary>
    public DateTime UpdatedTime { get; internal set; } = DateTime.Now;
}
