using ThingsEdge.Providers.Ops.Exchange.Monitors;

namespace ThingsEdge.Providers.Ops.Exchange.Policies;

/// <summary>
/// 默认心跳监控策略
/// </summary>
internal sealed class DefaultHeartbeatMonitorPolicy : IMonitorPolicy
{
    public bool Validate(PayloadData data)
    {
        return AbstractMonitor.CheckOn(data);
    }

    public object? Return(Tag tag)
    {
        return AbstractMonitor.SetOff2(tag);
    }
}
