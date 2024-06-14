using ThingsEdge.Contrib.Http.Model;

namespace ThingsEdge.Contrib.Http.Events;

/// <summary>
/// 目标服务（下游服务）健康状况检测通知事件。
/// </summary>
/// <param name="HealthState">目标健康状况。</param>
public sealed record DestinationHealthCheckedEvent(DestinationHealthState HealthState) : INotification;
