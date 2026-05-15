using Proto;
using ThingsEdge.Exchange.Addresses;
using ThingsEdge.Exchange.Infrastructure.Actors;

namespace ThingsEdge.Exchange.Actors;

/// <summary>
/// 执行引擎 Actor。
/// </summary>
internal sealed class EngineActor(IAddressFactory addressFactory) : IActor
{
    public async Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case EngineStartMessage _:
                var devices = addressFactory.GetDevices();
                foreach (var device in devices)
                {
                    var actor = context.SpawnFor<DeviceActor>(device.DeviceId, props =>
                    {
                        props.WithChildSupervisorStrategy(new OneForOneStrategy((pid, reason) =>
                            {
                                return SupervisorDirective.Restart;
                            },
                        3,
                        TimeSpan.FromSeconds(10)));
                    }, [device]);

                    context.Send(actor, new DeviceStartMessage()); // 告知设备 Actor 启动
                }

                break;

            case EngineStopMessage stopMsg:
                foreach (var pid in context.Children)
                {
                    context.Stop(pid);
                }

                break;
        }
    }
}

/// <summary>
/// 引擎启动消息。
/// </summary>
public sealed record EngineStartMessage();

/// <summary>
/// 引擎停止消息。
/// </summary>
public sealed record EngineStopMessage(string? Name = null);
