namespace ThingsEdge.Router;

/// <summary>
/// 交换机引擎。
/// </summary>
public interface IExchange : IAsyncDisposable
{
    /// <summary>
    /// 获取运行状态，是否正在运行中。
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// 启动。
    /// </summary>
    /// <returns></returns>
    Task StartAsync();

    /// <summary>
    /// 引擎停止。
    /// </summary>
    Task ShutdownAsync();
}
