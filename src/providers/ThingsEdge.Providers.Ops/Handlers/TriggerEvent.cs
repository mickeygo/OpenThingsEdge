using ThingsEdge.Providers.Ops.Exchange;

namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 标记 <see cref="Contracts.Devices.TagFlag.Trigger"/> 事件。
/// </summary>
internal sealed class TriggerEvent : INotification
{
    [NotNull]
    public DriverConnector? Connector { get; init; }

    [NotNull]
    public string? ChannelName { get; init; }

    [NotNull]
    public Device? Device { get; init; }

    [NotNull]
    public Tag? Tag { get; init; }

    [NotNull]
    public PayloadData? Self { get; init; }
}
