namespace ThingsEdge.Contracts;

/// <summary>
/// 驱动提供者。
/// </summary>
public interface IDriverProvider
{
    /// <summary>
    /// 数据处理命令。
    /// </summary>
    IDataCommand Command { get; }

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
