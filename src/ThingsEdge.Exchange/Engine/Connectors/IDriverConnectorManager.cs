using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Engine.Connectors;

/// <summary>
/// 驱动连接器管理者。
/// </summary>
internal interface IDriverConnectorManager : IDisposable
{
    /// <summary>
    /// 获取指定的连接驱动
    /// </summary>
    /// <param name="deviceId">设备Id</param>
    /// <returns></returns>
    IDriverConnector? GetConnector(string deviceId);

    /// <summary>
    /// 获取所有的连接驱动
    /// </summary>
    /// <returns></returns>
    IReadOnlyCollection<IDriverConnector> GetAllDriver();

    /// <summary>
    /// 加载驱动。
    /// </summary>
    /// <param name="deviceInfos"></param>
    void Load(IEnumerable<Device> deviceInfos);

    /// <summary>
    /// 驱动连接到服务
    /// </summary>
    /// <returns></returns>
    Task ConnectAsync();

    /// <summary>
    /// 驱动连接挂起。
    /// </summary>
    void Suspend();

    /// <summary>
    /// 恢复正常运行状态。
    /// </summary>
    void Recovery();

    /// <summary>
    /// 关闭并释放所有连接，同时会清空连接缓存。
    /// </summary>
    void Close();
}
