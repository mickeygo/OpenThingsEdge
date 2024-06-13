namespace ThingsEdge.Router.Forwarders;

/// <summary>
/// 通知数据转发包装接口。
/// </summary>
internal interface INotificationForwarderWrapper
{
    /// <summary>
    /// 发送通知。
    /// </summary>
    /// <param name="requestMessage">请求消息</param>
    /// <param name="lastMasterPayloadData">最近一次记录的标记主数据，没有则为 null</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishAsync(RequestMessage requestMessage, PayloadData? lastMasterPayloadData, CancellationToken cancellationToken = default);
}
