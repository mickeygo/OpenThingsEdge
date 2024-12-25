using Nito.AsyncEx;
using ThingsEdge.Exchange.Configuration;
using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Engine.Connectors;
using ThingsEdge.Exchange.Engine.Messages;
using ThingsEdge.Exchange.Infrastructure.Brokers;

namespace ThingsEdge.Exchange.Engine.Workers;

/// <summary>
/// 监控开关的工作者。
/// </summary>
internal sealed class SwitchWorker(
    IMessageBroker<SwitchMessage> broker,
    IOptions<ExchangeOptions> options,
    ILogger<SwitchWorker> logger) : IWorker
{
    public Task RunAsync(IDriverConnector connector, string channelName, Device device, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        var tags = device.GetAllTags(TagFlag.Switch);
        foreach (var tag in tags)
        {
            AsyncManualResetEvent mre = new(false); // 手动事件

            // 开关绑定的数据
            _ = Task.Run(async () =>
            {
                var pollingInterval = options.Value.SwitchScanRate > 0 ? options.Value.SwitchScanRate : 71;
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // 第一次检测
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        await Task.Delay(pollingInterval, cancellationToken).ConfigureAwait(false);

                        // 第二次检测
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        if (!connector.CanConnect)
                        {
                            continue;
                        }

                        // 发布触发事件
                        await broker.PushAsync(new SwitchMessage(connector, channelName, device, tag, null), cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "[SwitchWorker] Switch 数据读取异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}",
                                device.Name, tag.Name, tag.Address);
                    }
                }
            }, default);

            // 开关状态监控
            _ = Task.Run(async () =>
            {
                var pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : options.Value.DefaultScanRate;
                bool isOn = false; // 开关处于的状态
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(pollingInterval, cancellationToken).ConfigureAwait(false);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        if (!connector.CanConnect)
                        {
                            continue;
                        }

                        var (ok, data, _) = await connector.ReadAsync(tag).ConfigureAwait(false);
                        // 读取成功且开关处于 on 状态，发送开启动作信号。
                        if (ok)
                        {
                            if (WorkerUtils.CheckSwitchOn(data!)) // Open 标记
                            {
                                // Open 信号，在本身处于关闭状态，才执行开启动作。
                                if (!isOn)
                                {
                                    // 发送 On 信号结束标识
                                    await broker.PushAsync(new SwitchMessage(connector, channelName, device, tag, data!, SwitchState.On, true),
                                        cancellationToken).ConfigureAwait(false);

                                    // 开关开启时，发送信号，让子任务执行。
                                    isOn = true;
                                    mre.Set();
                                }
                            }
                            else
                            {
                                // Close 标记，在本身处于开启状态，才执行关闭动作。
                                if (isOn)
                                {
                                    // 发送 Off 信号结束标识事件
                                    await broker.PushAsync(new SwitchMessage(connector, channelName, device, tag, data!, SwitchState.Off, true),
                                        cancellationToken).ConfigureAwait(false);

                                    // 读取失败或开关关闭时，重置信号，让子任务阻塞。
                                    isOn = false;
                                    mre.Reset();
                                }
                            }

                            // 跳转
                            continue;
                        }

                        // 若读取失败，且开关处于 on 状态，则发送关闭动作信号（防止因设备未掉线，而读取失败导致一直发送数据）。
                        if (isOn)
                        {
                            // 发送 Off 信号结束标识事件
                            await broker.PushAsync(new SwitchMessage(
                                connector,
                                channelName,
                                device,
                                tag,
                                WorkerUtils.CreateSwitchPayloadOffOff(tag),
                                SwitchState.Off,
                                true), cancellationToken).ConfigureAwait(false);

                            // 读取失败或开关关闭时，重置信号，让子任务阻塞。
                            isOn = false;
                            mre.Reset();
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "[SwitchWorker] Switch 开关数据处理异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}",
                               device.Name, tag.Name, tag.Address);
                    }
                }

                // 任务取消后，无论什么情况都发送信号，确保让子任务也能退出
                mre.Set();
            }, default);
        }

        return Task.CompletedTask;
    }
}
