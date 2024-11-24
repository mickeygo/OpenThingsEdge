using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Engine.Workers;

/// <summary>
/// 表示工作器基础类。
/// </summary>
internal interface IWorker
{
    /// <summary>
    /// 运行。
    /// </summary>
    /// <param name="connector">驱动连接器</param>
    /// <param name="channelName">通道名称</param>
    /// <param name="device">设备</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RunAsync(IDriverConnector connector, string channelName, Device device, CancellationToken cancellationToken);
}
