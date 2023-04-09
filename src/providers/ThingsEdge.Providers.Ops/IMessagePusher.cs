using ThingsEdge.Providers.Ops.Exchange;

namespace ThingsEdge.Providers.Ops;

public interface IMessagePusher
{
    /// <summary>
    /// 推送消息
    /// </summary>
    /// <param name="connector">连接器</param>
    /// <param name="tag">标记</param>
    /// <param name="self">自身数据</param>
    /// <returns></returns>
    Task PushAsync(DriverConnector connector, Tag tag, PayloadData? self);
}
