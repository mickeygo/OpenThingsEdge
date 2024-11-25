namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 通知数据转发代理接口。
/// </summary>
internal interface INoticeForwarderProxy
{
    /// <summary>
    /// 发送通知。
    /// </summary>
    /// <param name="context">通知消息上下文</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishAsync(NoticeContext context, CancellationToken cancellationToken = default);
}
