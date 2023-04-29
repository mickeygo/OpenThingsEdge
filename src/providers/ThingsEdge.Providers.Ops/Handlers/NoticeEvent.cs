using ThingsEdge.Providers.Ops.Exchange;

namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 标记 <see cref="Contracts.Devices.TagFlag.Notice"/> 事件。
/// </summary>
internal sealed class NoticeEvent : INotification
{
    /// <summary>
    /// 连接驱动
    /// </summary>
    [NotNull]
    public DriverConnector? Connector { get; init; }

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
    /// 读取的标记值
    /// </summary>
    [NotNull]
    public PayloadData? Self { get; init; }
}
