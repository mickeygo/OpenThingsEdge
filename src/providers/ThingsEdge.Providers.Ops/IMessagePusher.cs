using ThingsEdge.Providers.Ops.Exchange;

namespace ThingsEdge.Providers.Ops;

/// <summary>
/// 数据推送。
/// </summary>
public interface IMessagePusher
{
    /// <summary>
    /// 推送消息
    /// </summary>
    /// <param name="connector">连接器。</param>
    /// <param name="device">设备。</param>
    /// <param name="tag">标记。</param>
    /// <param name="self">自身数据，不存在时设为空。</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PushAsync(DriverConnector connector, Device device, Tag tag, PayloadData? self = null, CancellationToken cancellationToken = default);
}
