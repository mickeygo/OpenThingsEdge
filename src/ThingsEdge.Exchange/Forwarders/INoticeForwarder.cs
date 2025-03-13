using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 通知数据传送接口，其中 <see cref="TagFlag.Notice"/> 会发布此事件。
/// </summary>
public interface INoticeForwarder
{
    /// <summary>
    /// 接收通知消息。
    /// </summary>
    /// <param name="context">通知消息上下文</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ReceiveAsync(NoticeContext context, CancellationToken cancellationToken = default);
}
