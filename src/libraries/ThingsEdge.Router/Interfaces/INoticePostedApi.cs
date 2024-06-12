namespace ThingsEdge.Router.Interfaces;

/// <summary>
/// 通知数据发布接口，只针对于 <see cref="TagFlag.Notice"/>。
/// </summary>
/// <remarks>服务端采用 <see cref="ServiceLifetime.Scoped"/> 作用域来解析服务。</remarks>
public interface INoticePostedApi
{
    /// <summary>
    /// 发送通知。
    /// </summary>
    /// <param name="requestMessage">请求消息</param>
    /// <param name="lastMasterPayloadData">最近一次记录的标记主数据，没有则为 null</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task NotifyAsync(RequestMessage requestMessage, PayloadData? lastMasterPayloadData, CancellationToken cancellationToken);
}
