namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 设备心跳数据推送接口代理。
/// </summary>
public interface IHeartbeatForwarderProxy
{
    /// <summary>
    /// 更改设备心跳状态。
    /// </summary>
    /// <param name="context">是否为在线状态</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ChangeAsync(HeartbeatContext context, CancellationToken cancellationToken);
}
