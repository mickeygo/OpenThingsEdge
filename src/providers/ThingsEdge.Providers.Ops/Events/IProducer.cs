namespace ThingsEdge.Providers.Ops.Events;

/// <summary>
/// 消息内部事件生产者，要发布是事件需实现 <see cref="IMonitorEvent"/> 接口。
/// </summary>
internal interface IProducer
{
    /// <summary>
    /// 生产事件
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask ProduceAsync(IMonitorEvent item, CancellationToken cancellationToken = default);
}
