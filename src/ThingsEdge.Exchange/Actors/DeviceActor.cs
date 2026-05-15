using Proto;
using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Engine.Connectors;
using ThingsEdge.Exchange.Infrastructure.Actors;

namespace ThingsEdge.Exchange.Actors;

/// <summary>
/// 设备 Actor
/// </summary>
internal sealed class DeviceActor(Device device) : IActor
{
    private IDriverConnector? _driverConnector;

    public async Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case DeviceStartMessage _:
                var actor = context.SpawnFor<DeviceConnectorActor>([device]);
                // 连接设备
                _driverConnector = await context.RequestAsync<IDriverConnector>(actor, new DeviceConnectorCreateAndConnectMessage()).ConfigureAwait(false);

                // 监控点位
                break;
        }
    }
}

/// <summary>
/// 设备启动消息。
/// </summary>
public sealed record DeviceStartMessage;
