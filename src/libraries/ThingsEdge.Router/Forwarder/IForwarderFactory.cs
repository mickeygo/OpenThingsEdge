namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// 数据传送工厂。
/// </summary>
public interface IForwarderFactory
{
    /// <summary>
    /// 发送数据。
    /// </summary>
    /// <param name="message">要发送的数据。</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResponseResult> SendAsync(RequestMessage message, CancellationToken cancellationToken = default);
}
