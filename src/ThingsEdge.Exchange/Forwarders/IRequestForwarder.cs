using ThingsEdge.Exchange.Contracts;
using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 请求数据传送接口，其中仅有 <see cref="TagFlag.Trigger"/> 会发布此事件。
/// </summary>
public interface IRequestForwarder
{
    /// <summary>
    /// 发送数据
    /// </summary>
    /// <param name="message">请求的数据</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResponseResult> SendAsync(RequestMessage message, CancellationToken cancellationToken = default);
}
