using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 通知数据传送接口，其中仅有 <see cref="TagFlag.Notice"/> 会发布此事件。
/// </summary>
public interface INoticeForwarder
{
    /// <summary>
    /// 发送通知。
    /// </summary>
    /// <param name="context">通知消息上下文</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishAsync(NoticeContext context, CancellationToken cancellationToken = default);
}
