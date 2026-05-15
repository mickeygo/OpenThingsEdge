using Proto;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Exchange.Engine.Connectors;

namespace ThingsEdge.Exchange.Actors;

/// <summary>
/// 设备连接器健康检查 Actor。
/// </summary>
internal sealed class DeviceConnectorHealthCheckActor(IDriverConnector driverConnector) : IActor
{
    public async Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case DeviceConnectorHealthCheckMessage _:
                var connector = driverConnector;
                if (connector != null && connector.ConnectedStatus is not ConnectionStatus.Aborted && connector.Driver is DeviceTcpNet networkDevice)
                {
                    try
                    {
                        connector.Available = await networkDevice.PingSuccessfulAsync(1_000).ConfigureAwait(false);
                        if (connector.Available && connector.ConnectedStatus == ConnectionStatus.Disconnected)
                        {
                            // 内部 Socket 异常，或是第一次尝试连接过服务器失败
                            if (networkDevice.IsSocketError)
                            {
                                var result = await networkDevice.ConnectServerAsync().ConfigureAwait(false);
                                if (result.IsSuccess)
                                {
                                    connector.ConnectedStatus = ConnectionStatus.Connected;
                                    // 记录 已连接上服务 日志
                                }
                            }
                        }
                    }
                    catch (Exception ex) when (ex is PingException)
                    {
                        connector.Available = false;
                        // 记录 Ping 服务器异常 日志
                    }
                    catch (Exception)
                    {
                        connector.Available = false;
                        // 记录异常日志
                    }
                }

                // 等待指定时间后重新发起健康检查消息
                context.ReenterAfter(Task.Delay(10_000, context.CancellationToken), () =>
                {
                    context.Send(context.Self, new DeviceConnectorHealthCheckMessage());
                });

                break;
        }
    }
}

/// <summary>
/// 设备连接健康检查消息。
/// </summary>
public sealed record DeviceConnectorHealthCheckMessage;
