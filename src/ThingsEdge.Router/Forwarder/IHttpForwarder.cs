namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// 表示是基于 Http 协议转发请求数据。
/// </summary>
public interface IHttpForwarder
{
    /// <summary>
    /// 发送数据。
    /// </summary>
    /// <param name="message">要发送的数据。</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResult> SendAsync(RequestMessage message, CancellationToken cancellationToken);
}
