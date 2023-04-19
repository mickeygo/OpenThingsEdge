namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 标记 <see cref="Contracts.Devices.TagFlag.Heartbeat"/> 事件。
/// </summary>
internal sealed class HeartbeatEvent : INotification
{
    [NotNull]
    public Device? Device { get; init; }

    [NotNull]
    public Tag? Tag { get; init; }
}
