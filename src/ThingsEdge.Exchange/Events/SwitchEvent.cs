using ThingsEdge.Exchange.Contracts;
using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Engine;

namespace ThingsEdge.Exchange.Events;

/// <summary>
/// 标记 <see cref="TagFlag.Switch"/> 事件。
/// </summary>
internal sealed class SwitchEvent : INotification, IMonitorEvent
{
    /// <summary>
    /// 连接驱动
    /// </summary>
    [NotNull]
    public IDriverConnector? Connector { get; init; }

    /// <summary>
    /// 通道名称
    /// </summary>
    [NotNull]
    public string? ChannelName { get; init; }

    [NotNull]
    public Device? Device { get; init; }

    /// <summary>
    /// 标记
    /// </summary>
    [NotNull]
    public Tag? Tag { get; init; }

    /// <summary>
    /// 读取的标记值，开关开启/闭合时才有值。
    /// </summary>
    [NotNull]
    public PayloadData? Self { get; init; }

    /// <summary>
    /// 开关状态。
    /// </summary>
    public SwitchState State { get; init; } = SwitchState.None;

    /// <summary>
    /// 是否为开关信号。
    /// 注：开关标识，只表示数据为开关信号，否则为开关下具体的数据。
    /// </summary>
    public bool IsSwitchSignal { get; init; }
}

/// <summary>
/// 开关状态。
/// </summary>
public enum SwitchState
{
    /// <summary>
    /// 未知
    /// </summary>
    None,

    /// <summary>
    /// 开启
    /// </summary>
    On,

    /// <summary>
    /// 关闭
    /// </summary>
    Off,
}
