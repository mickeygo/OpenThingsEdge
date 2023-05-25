using ThingsEdge.Contracts.Variables;

namespace ThingsEdge.Router.Interfaces;

/// <summary>
/// 设备心跳接口。
/// </summary>
public interface IDeviceHeartbeatApi
{
    /// <summary>
    /// 更改设备心跳状态。
    /// </summary>
    /// <param name="channelName">通道名称</param>
    /// <param name="device">设备</param>
    /// <param name="tag">标记</param>
    /// <param name="isOnline">是否为在线状态</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ChangeAsync(string channelName, Device device, Tag tag, bool isOnline, CancellationToken cancellationToken);
}
