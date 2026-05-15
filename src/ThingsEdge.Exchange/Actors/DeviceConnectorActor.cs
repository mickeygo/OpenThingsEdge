using Proto;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Exchange.Connectors;
using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Engine.Connectors;
using ThingsEdge.Exchange.Infrastructure.Actors;

namespace ThingsEdge.Exchange.Actors;

/// <summary>
/// 设备连接器 Actor。
/// </summary>
internal sealed class DeviceConnectorActor(
    IDriverConnectorManager2 driverConnectorManager,
    Device device) : IActor
{
    private IDriverConnector? _driverConnector;

    public async Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case DeviceConnectorCreateAndConnectMessage _:
                // 创建并连接
                _driverConnector = await driverConnectorManager.CreateAndConnectAsync(device).ConfigureAwait(false);

                // 将连接器回传给发送者
                context.Respond(_driverConnector);

                // 等待指定时间后，启用健康检查
                context.ReenterAfter(Task.Delay(5_000, context.CancellationToken), () =>
                {
                    var actor = context.SpawnFor<DeviceConnectorHealthCheckActor>([_driverConnector]);
                    context.Send(actor, new DeviceConnectorHealthCheckMessage());
                });

                break;

            case Stopping _:
                if (_driverConnector != null && _driverConnector is not { ConnectedStatus: ConnectionStatus.Aborted })
                {
                    _driverConnector.ConnectedStatus = ConnectionStatus.Aborted;
                    if (_driverConnector.Driver is DeviceTcpNet networkDevice2)
                    {
                        networkDevice2.Close();
                    }
                }

                break;
        }
    }
}

/// <summary>
/// 设备连接消息。
/// </summary>
public sealed record DeviceConnectorCreateAndConnectMessage;
