namespace ThingsEdge.Providers.Ops.Exchange;

public interface IEngine : IDisposable
{
    /// <summary>
    /// 获取运行状态，是否正在运行中。
    /// </summary>
    bool IsRuning { get; }

    /// <summary>
    /// 启动。
    /// </summary>
    /// <returns></returns>
    Task StartAsync();

    /// <summary>
    /// 引擎停止。
    /// </summary>
    void Stop();
}
