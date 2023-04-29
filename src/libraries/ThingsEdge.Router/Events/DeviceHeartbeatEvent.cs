using ThingsEdge.Router.Model;

namespace ThingsEdge.Router.Events;

/// <summary>
/// 与设备连接的心跳事件。
/// </summary>
public sealed class DeviceHeartbeatEvent : INotification
{
    [NotNull]
    public string? ChannelName { get; init; }

    [NotNull]
    public Device? Device { get; init; }

    [NotNull]
    public Tag? Tag { get; init; }

    /// <summary>
    /// 设备连接状态。
    /// </summary>
    public DeviceConnectState ConnectState { get; init; }
}
