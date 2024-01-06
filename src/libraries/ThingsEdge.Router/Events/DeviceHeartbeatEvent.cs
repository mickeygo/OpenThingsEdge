namespace ThingsEdge.Router.Events;

/// <summary>
/// 与设备连接的心跳通知事件，其中 <see cref="TagFlag.Heartbeat"/> 会发布此事件
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
    /// 是否为在线状态。
    /// </summary>
    public bool IsOnline { get; init; }

    public static DeviceHeartbeatEvent Create(string channelName, Device device, Tag tag, bool isOnline = false)
    {
        return new DeviceHeartbeatEvent
        {
            ChannelName = channelName,
            Device = device,
            Tag = tag,
            IsOnline = isOnline,
        };
    }
}
