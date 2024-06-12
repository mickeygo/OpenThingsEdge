namespace ThingsEdge.Providers.Ops.Events;

/// <summary>
/// 引擎状态更改事件
/// </summary>
/// <param name="State">引擎运行状态</param>
internal sealed record ExchangeChangedEvent(RunningState State) : INotification, IMonitorEvent;

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
