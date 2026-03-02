using ThingsEdge.Exchange.Forwarders;

namespace ThingsEdge.Silos.Client.Forwarders;

internal sealed class HeartbeatForwarder(ILogger<HeartbeatForwarder> logger) : IHeartbeatForwarder
{
    public Task ReceiveAsync(HeartbeatContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation("心跳监控，设备名称：{DeviceName}，状态：{State}", context.Device.Name, context.IsOnline ? "on" : "off");

        return Task.CompletedTask;
    }
}
