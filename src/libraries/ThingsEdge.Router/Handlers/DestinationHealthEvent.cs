namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 目标健康状况事件。
/// </summary>
public sealed class DestinationHealthEvent : INotification
{
    /// <summary>
    /// 目标健康状况。
    /// </summary>
    public DestinationHealthState HealthState { get; init; }
}
