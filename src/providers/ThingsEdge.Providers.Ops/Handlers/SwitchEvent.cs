using ThingsEdge.Providers.Ops.Exchange;

namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 标记 <see cref="Contracts.Devices.TagFlag.Switch"/> 事件。
/// </summary>
internal sealed class SwitchEvent : INotification
{
    [NotNull]
    public DriverConnector? Connector { get; init; }

    [NotNull]
    public string? ChannelName { get; init; }

    [NotNull]
    public Device? Device { get; init; }

    [NotNull]
    public Tag? Tag { get; init; }

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