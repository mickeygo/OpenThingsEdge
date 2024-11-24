using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 心跳传送数据上下文。
/// </summary>
public sealed class HeartbeatForwarderContext
{
    /// <summary>
    /// 通道名称
    /// </summary>
    public string? ChannelName { get; init; }

    /// <summary>
    /// 设备
    /// </summary>
    [NotNull]
    public Device? Device { get; init; }

    /// <summary>
    /// 标记
    /// </summary>
    [NotNull]
    public Tag? Tag { get; init; }

    /// <summary>
    /// 是否在线
    /// </summary>
    public bool IsOnline { get; init; }
}
