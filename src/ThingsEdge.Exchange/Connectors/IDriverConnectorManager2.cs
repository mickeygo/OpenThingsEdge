using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Engine.Connectors;

namespace ThingsEdge.Exchange.Connectors;

/// <summary>
/// 设备驱动连接管理器。
/// </summary>
internal interface IDriverConnectorManager2
{
    /// <summary>
    /// 创建连接器并尝试连接设备
    /// </summary>
    /// <param name="deviceInfo">要连接的设备信息</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// 返回驱动连接器，更新 ConnectedStatus 状态判断是否有连接。
    /// </returns>
    Task<IDriverConnector> CreateAndConnectAsync(Device deviceInfo, CancellationToken cancellationToken = default);
}
