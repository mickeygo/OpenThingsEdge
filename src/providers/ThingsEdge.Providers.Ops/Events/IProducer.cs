using ThingsEdge.Router.Events;

namespace ThingsEdge.Providers.Ops.Events;

/// <summary>
/// 消息事件生产者
/// </summary>
internal interface IProducer
{
    /// <summary>
    /// 生产事件
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask ProduceAsync(IEvent item, CancellationToken cancellationToken = default);
}
