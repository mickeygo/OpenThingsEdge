using ThingsEdge.Exchange.Contracts;

namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 触发数据转发代理接口。
/// </summary>
internal interface ITriggerForwarderProxy
{
    /// <summary>
    /// 发送请求数据。
    /// </summary>
    /// <param name="message">请求的数据</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResponseResult> SendAsync(RequestMessage message, CancellationToken cancellationToken = default);
}
