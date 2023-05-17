namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 标记 <see cref="TagFlag.Heartbeat"/> 事件。
/// </summary>
internal sealed class HeartbeatEvent : INotification
{
    /// <summary>
    /// 通道名称
    /// </summary>
    [NotNull]
    public string? ChannelName { get; init; }

    /// <summary>
    /// 设备信息
    /// </summary>
    [NotNull]
    public Device? Device { get; init; }

    /// <summary>
    /// 标记
    /// </summary>
    [NotNull]
    public Tag? Tag { get; init; }

    /// <summary>
    /// 是否处于连接状态
    /// </summary>
    public bool IsConnected { get; init; }

    public static HeartbeatEvent Create(string channelName, Device device, Tag tag, bool isConnected = false)
    {
        return new HeartbeatEvent
        {
            ChannelName = channelName,
            Device = device,
            Tag = tag,
            IsConnected = isConnected,
        };
    }
}
