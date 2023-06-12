using ThingsEdge.Providers.Ops.Configuration;
using ThingsEdge.Providers.Ops.Events;

namespace ThingsEdge.Providers.Ops.Exchange.Monitors;

/// <summary>
/// 开关监控
/// </summary>
internal sealed class SwitchMonitor : AbstractMonitor, ITransientDependency
{
    private readonly IProducer _producer;
    private readonly OpsConfig _opsConfig;
    private readonly ILogger _logger;

    public SwitchMonitor(IProducer producer,
        IOptionsMonitor<OpsConfig> opsConfig,
        ILogger<SwitchMonitor> logger)
    {
        _producer = producer;
        _opsConfig = opsConfig.CurrentValue;
        _logger = logger;
    }

    public override void Monitor(IDriverConnector connector, string channelName, Device device, CancellationToken cancellationToken)
    {
        var tags = device.GetAllTags(TagFlag.Switch);
        foreach (var tag in tags)
        {
            AsyncManualResetEvent mre = new(false); // 手动事件

            // 开关绑定的数据
            _ = Task.Run(async () =>
            {
                int pollingInterval = _opsConfig.SwitchScanRate > 0 ? _opsConfig.SwitchScanRate : 70;
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await mre.WaitAsync().ConfigureAwait(false);

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

                        // 记录数据
                        await _producer.ProduceAsync(new SwitchEvent { Connector = connector, ChannelName = channelName, Device = device, Tag = tag }).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[SwitchMonitor] Switch 数据读取异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}",
                                device.Name, tag.Name, tag.Address);
                    }
                }
            });

            // 开关状态监控
            _ = Task.Run(async () =>
            {
                int pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _opsConfig.DefaultScanRate;
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
                            if (CheckOn(data!)) // Open 标记
                            {
                                // Open 信号，在本身处于关闭状态，才执行开启动作。
                                if (!isOn)
                                {
                                    // 发送 On 信号结束标识
                                    await _producer.ProduceAsync(new SwitchEvent
                                    {
                                        Connector = connector,
                                        ChannelName = channelName,
                                        Device = device,
                                        Tag = tag,
                                        Self = data,
                                        State = SwitchState.On,
                                        IsSwitchSignal = true,
                                    }).ConfigureAwait(false);

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
                                    await _producer.ProduceAsync(new SwitchEvent
                                    {
                                        Connector = connector,
                                        ChannelName = channelName,
                                        Device = device,
                                        Tag = tag,
                                        Self = data,
                                        State = SwitchState.Off,
                                        IsSwitchSignal = true,
                                    }).ConfigureAwait(false);

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
                            await _producer.ProduceAsync(new SwitchEvent
                            {
                                Connector = connector,
                                ChannelName = channelName,
                                Device = device,
                                Tag = tag,
                                Self = SetOff(tag),
                                State = SwitchState.Off,
                                IsSwitchSignal = true,
                            }).ConfigureAwait(false);

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
                        _logger.LogError(ex, "[SwitchMonitor] Switch 开关数据处理异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}",
                               device.Name, tag.Name, tag.Address);
                    }
                }

                // 任务取消后，无论什么情况都发送信号，确保让子任务也能退出
                mre.Set();
            });
        }
    }
}
