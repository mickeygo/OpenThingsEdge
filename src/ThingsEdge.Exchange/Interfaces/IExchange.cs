namespace ThingsEdge.Exchange.Interfaces;

/// <summary>
/// 交换机引擎。
/// </summary>
public interface IExchange : IAsyncDisposable
{
    /// <summary>
    /// 获取引擎运行状态，是否正在运行中。
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// 启动引擎。
    /// </summary>
    /// <returns></returns>
    Task StartAsync();

    /// <summary>
    /// 引擎停止。
    /// </summary>
    Task ShutdownAsync();
}
