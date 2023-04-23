namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// 转发请求数据。
/// </summary>
public interface IForwarder
{
    /// <summary>
    /// 发送数据。
    /// </summary>
    /// <param name="message">要发送的数据。</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResponseResult> SendAsync(RequestMessage message, CancellationToken cancellationToken = default);
}
