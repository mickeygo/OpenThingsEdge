namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// 表示为 Native 转发服务接口，其中 <see cref="TagFlag.Notice"/> 和 <see cref="TagFlag.Trigger"/> 会发布此事件。
/// </summary>
public interface INativeForwarder
{
    /// <summary>
    /// 发送数据
    /// </summary>
    /// <param name="message">请求的数据</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResponseMessage> SendAsync(RequestMessage message, CancellationToken cancellationToken = default);
}
