using ThingsEdge.Contracts.Variables;
using ThingsEdge.Router.Interfaces;

namespace ThingsEdge.App.Handlers;

/// <summary>
/// 设备心跳信息处理。
/// </summary>
internal sealed class HeartbeatApiHandler(ILogger<HeartbeatApiHandler> logger) : IDeviceHeartbeatApi
{
    public Task ChangeAsync(string channelName, Device device, Tag tag, bool isOnline, CancellationToken cancellationToken)
    {
        logger.LogInformation("心跳监控，设备名称：{DeviceName}，状态：{State}", device.Name, isOnline ? "on" : "off");

        return Task.CompletedTask;
    }
}
