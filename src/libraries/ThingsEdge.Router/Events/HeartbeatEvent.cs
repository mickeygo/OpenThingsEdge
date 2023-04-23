using ThingsEdge.Router.Model;

namespace ThingsEdge.Router.Events;

/// <summary>
/// 心跳事件。
/// </summary>
public sealed class HeartbeatEvent : INotification
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
