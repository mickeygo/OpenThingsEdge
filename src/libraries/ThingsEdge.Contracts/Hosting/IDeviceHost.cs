namespace ThingsEdge.Contracts.Hosting;

public interface IDeviceHost
{
    /// <summary>
    /// 自身是是否为引擎。
    /// </summary>
    bool IsEngine { get; }

    /// <summary>
    /// 运行引擎。
    /// </summary>
    /// <returns></returns>
    Task RunAsync();

    /// <summary>
    /// 停止运行。
    /// </summary>
    /// <returns></returns>
    Task ShutdownAsync();
}
