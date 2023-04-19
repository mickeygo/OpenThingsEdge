using ThingsEdge.Providers.Ops.Exchange;

namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 标记 <see cref="Contracts.Devices.TagFlag.Notice"/> 事件。
/// </summary>
internal sealed class NoticeEvent : INotification
{
    [NotNull]
    public DriverConnector? Connector { get; init; }

    [NotNull]
    public Device? Device { get; init; }

    [NotNull]
    public Tag? Tag { get; init; }

    [NotNull]
    public PayloadData? Self { get; init; }
}
