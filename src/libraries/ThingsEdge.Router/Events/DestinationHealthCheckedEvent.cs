﻿using ThingsEdge.Router.Model;

namespace ThingsEdge.Router.Events;

/// <summary>
/// 目标服务（下游服务）健康状况检测通知事件。
/// </summary>
public sealed class DestinationHealthCheckedEvent : INotification
{
    /// <summary>
    /// 事件创建时间。
    /// </summary>
    public DateTime EventTime { get; init; } = DateTime.Now;

    /// <summary>
    /// 目标健康状况。
    /// </summary>
    public DestinationHealthState HealthState { get; init; }
}
