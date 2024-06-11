using ThingsEdge.Contracts.Variables;
using ThingsEdge.Router.Interfaces;

namespace ThingsEdge.App.Handlers;

/// <summary>
/// 设备心跳信息处理。
/// </summary>
internal sealed class HeartbeatApiHandler : IDeviceHeartbeatApi
{
    public Task ChangeAsync(string channelName, Device device, Tag tag, bool isOnline, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
