using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 开关数据传送接口，其中 <see cref="TagFlag.Switch"/> 会发布此事件。
/// </summary>
public interface ISwitchForwarder
{
    /// <summary>
    /// 接收开关数据。
    /// </summary>
    /// <param name="context">开关消息上下文</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ReceiveAsync(SwitchContext context, CancellationToken cancellationToken = default);
}
