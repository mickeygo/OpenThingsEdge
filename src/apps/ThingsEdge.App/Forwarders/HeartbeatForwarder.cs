using ThingsEdge.Contracts.Variables;
using ThingsEdge.Router.Forwarders;

namespace ThingsEdge.App.Forwarders;

/// <summary>
/// 设备心跳信息处理。
/// </summary>
internal sealed class HeartbeatForwarder(ILogger<HeartbeatForwarder> logger) : INativeHeartbeatForwarder
{
    public Task ChangeAsync(string channelName, Device device, Tag tag, bool isOnline, CancellationToken cancellationToken)
    {
        logger.LogInformation("心跳监控，设备名称：{DeviceName}，状态：{State}", device.Name, isOnline ? "on" : "off");

        return Task.CompletedTask;
    }
}
