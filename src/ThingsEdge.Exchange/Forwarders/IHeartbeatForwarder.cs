using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 设备心跳数据推送接口，其中 <see cref="TagFlag.Heartbeat"/> 会发布此事件。
/// </summary>
public interface IHeartbeatForwarder
{
    /// <summary>
    /// 接收设备心跳状态信息。
    /// </summary>
    /// <param name="context">心跳传送数据上下文</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ReceiveAsync(HeartbeatContext context, CancellationToken cancellationToken);
}
