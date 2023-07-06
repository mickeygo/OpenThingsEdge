namespace ThingsEdge.Providers.Ops.Exchange.Monitors;

/// <summary>
/// Monitor 监控选项
/// </summary>
public sealed class MonitorOptions
{
    /// <summary>
    /// 设置心跳监控策略
    /// </summary>
    public Func<PayloadData, object>? HeartbeatMonitorPolicy { get; set; }

    /// <summary>
    /// 设置开关监控策略
    /// </summary>
    public Func<PayloadData, object>? SwitchMonitorPolicy { get; set; }
}
