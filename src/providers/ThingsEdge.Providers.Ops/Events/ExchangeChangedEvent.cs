namespace ThingsEdge.Providers.Ops.Events;

/// <summary>
/// 引擎状态更改事件
/// </summary>
internal sealed class ExchangeChangedEvent : INotification, IMonitorEvent
{
    /// <summary>
    /// 引擎状态
    /// </summary>
    public RunningState State { get; init; }
}

/// <summary>
/// 运行状态
/// </summary>
public enum RunningState
{
    /// <summary>
    /// 启动
    /// </summary>
    Startup,

    /// <summary>
    /// 停止
    /// </summary>
    Stop,
}