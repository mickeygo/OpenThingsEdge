namespace ThingsEdge.Providers.Ops.Exchange.Monitors;

/// <summary>
/// Monitor 监控选项
/// </summary>
public sealed class MonitorOptions
{
    public Func<PayloadData, object>? HeartbeatMonitorPolicy { get; set; }

    public Func<PayloadData, object>? SwitchMonitorPolicy { get; set; }
}
