using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 设备心跳数据推送接口，其中 <see cref="TagFlag.Heartbeat"/> 会发布此事件。
/// </summary>
public interface IHeartbeatForwarder
{
    /// <summary>
    /// 更改设备心跳状态。
    /// </summary>
    /// <param name="context">是否为在线状态</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ChangeAsync(HeartbeatContext context, CancellationToken cancellationToken);
}
