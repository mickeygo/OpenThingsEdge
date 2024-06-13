namespace ThingsEdge.Router.Forwarders;

/// <summary>
/// 通知数据传送接口，其中仅有 <see cref="TagFlag.Notice"/> 会发布此事件。
/// </summary>
/// <remarks>服务端采用 <see cref="ServiceLifetime.Scoped"/> 作用域来解析服务。</remarks>
public interface INotificationForwarder
{
    /// <summary>
    /// 发送通知。
    /// </summary>
    /// <param name="message">请求消息</param>
    /// <param name="lastMasterPayloadData">最近一次记录的标记主数据，没有则为 null</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishAsync(RequestMessage message, PayloadData? lastMasterPayloadData, CancellationToken cancellationToken = default);
}
