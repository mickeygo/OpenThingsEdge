using ThingsEdge.Common.EventBus;
using ThingsEdge.Providers.Ops.Configuration;
using ThingsEdge.Providers.Ops.Handlers;
using ThingsEdge.Router;
using ThingsEdge.Router.Devices;
using ThingsEdge.Router.Events;

namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// 数据交换引擎。
/// </summary>
public sealed class OpsExchange : IExchange, ISingletonDependency
{
    private CancellationTokenSource? _cts = new();

    private readonly IEventPublisher _publisher;
    private readonly IDeviceManager _deviceManager;
    private readonly DriverConnectorManager _driverConnectorManager;
    private readonly OpsConfig _opsConfig;
    private readonly ILogger _logger;

    public OpsExchange(IEventPublisher publisher,
        IDeviceManager deviceManager,
        DriverConnectorManager driverConnectorManager,
        IOptionsMonitor<OpsConfig> opsConfig,
        ILogger<OpsExchange> logger)
    {
        _publisher = publisher;
        _deviceManager = deviceManager;
        _driverConnectorManager = driverConnectorManager;
        _opsConfig = opsConfig.CurrentValue;
        _logger = logger;
    }

    public bool IsRunning { get; private set; }

    public async Task StartAsync()
    {
        if (IsRunning)
        {
            return;
        }
        IsRunning = true;

        _logger.LogInformation("[Engine] 引擎启动");
        await _publisher.Publish(LoggingMessageEvent.Info("[Engine] 引擎启动")).ConfigureAwait(false);

        _cts ??= new();

        var devices = _deviceManager.ReloadDevices();
        _driverConnectorManager.Load(devices);
        await _driverConnectorManager.ConnectAsync().ConfigureAwait(false);

        foreach (var connector in _driverConnectorManager.GetAllDriver())
        {
            // 心跳数据监控器
            _ = HeartbeatMonitorAsync(connector);

            // 触发数据监控器
            _ = TriggerMonitorAsync(connector);

            // 通知数据监控器
            _ = NoticeMonitorAsync(connector);

            // 开关数据监控器
            _ = SwitchMonitorAsync(connector);
        }
    }

    private Task HeartbeatMonitorAsync(IDriverConnector connector)
    {
        var (channelName, device) = _deviceManager.GetDevice2(connector.Id);
        var tags = device!.GetAllTags(TagFlag.Heartbeat); // 所有标记为心跳的都进行监控。
        foreach (var tag in tags)
        {
            _ = Task.Run(async () =>
            {
                int pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _opsConfig.DefaultScanRate;
                while (_cts != null && !_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(pollingInterval, _cts.Token).ConfigureAwait(false);

                        if (_cts == null)
                        {
                            if (!TagValueSet.CompareAndSwap(tag.TagId, false))
                            {
                                // 任务取消时，发布设备心跳断开事件。
                                await _publisher.Publish(HeartbeatEvent.Create(channelName!, device, tag, false, SetOff(tag)), 
                                    PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);
                            }

                            break;
                        }

                        if (!connector.CanConnect)
                        {
                            if(!TagValueSet.CompareAndSwap(tag.TagId, false))
                            {
                                // 连接断开时，发布设备心跳断开事件。
                                await _publisher.Publish(HeartbeatEvent.Create(channelName!, device, tag, false, SetOff(tag)), 
                                    PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);
                            }

                            continue;
                        }

                        var (ok, data, err) = await connector.ReadAsync(tag).ConfigureAwait(false);
                        if (!ok)
                        {
                            string msg = $"[Engine] Heartbeat 数据读取异常，设备：{device.Name}，标记：{tag.Name}, 地址：{tag.Address}，错误：{err}";
                            _logger.LogError(msg);
                            await _publisher.Publish(LoggingMessageEvent.Error(msg), PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);

                            continue;
                        }

                        // 心跳标记数据类型必须为 bool 或 int16
                        if (CheckOn(data!))
                        {
                            await connector.WriteAsync(tag, SetOff2(tag)).ConfigureAwait(false);

                            if (!TagValueSet.CompareAndSwap(tag.TagId, true))
                            {
                                // 发布心跳正常事件。
                                await _publisher.Publish(HeartbeatEvent.Create(channelName!, device, tag, true, data!), 
                                    PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);
                            }
                        }

                        // 发布心跳信号事件（仅记录值）。
                        await _publisher.Publish(HeartbeatEvent.Create(channelName!, device, tag, true, data!, true),
                            PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        string msg = $"[Engine] Heartbeat 数据处理异常，设备：{device.Name}，标记：{tag.Name}, 地址：{tag.Address}";
                        _logger.LogError(ex, msg);
                        await _publisher.Publish(LoggingMessageEvent.Error(msg), PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);
                    }
                }
            });
        }

        return Task.CompletedTask;
    }

    private Task TriggerMonitorAsync(IDriverConnector connector)
    {
        var (channelName, device) = _deviceManager.GetDevice2(connector.Id);
        var tags = device!.GetAllTags(TagFlag.Trigger);
        foreach (var tag in tags)
        {
            _ = Task.Run(async () =>
            {
                int pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _opsConfig.DefaultScanRate;
                while (_cts != null && !_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(pollingInterval, _cts.Token).ConfigureAwait(false);

                        if (_cts == null)
                        {
                            break;
                        }

                        if (!connector.CanConnect)
                        {
                            continue;
                        }

                        // 若读取失败，该信号点不会复位，下次会继续读取执行。
                        var (ok, data, err) = await connector.ReadAsync(tag).ConfigureAwait(false); // short 类型
                        if (!ok)
                        {
                            string msg = $"[Engine] Trigger 数据读取异常，设备：{device.Name}，标记：{tag.Name}, 地址：{tag.Address}，错误：{err}";
                            _logger.LogError(msg);
                            await _publisher.Publish(LoggingMessageEvent.Error(msg), PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);

                            continue;
                        }

                        // 校验触发标记
                        var state = data!.GetInt(); // 触发标记还可能包含状态码信息。

                        // 必须先检测并更新标记状态值，若值有变动且触发标记值为 1 则推送数据。
                        if (!TagValueSet.CompareAndSwap(tag.TagId, state) && state == 1)
                        {
                            // 发布触发事件
                            await _publisher.Publish(new TriggerEvent
                            {
                                Connector = connector,
                                ChannelName = channelName,
                                Device = device,
                                Tag = tag,
                                Self = data,
                            }).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        string msg = $"[Engine] Trigger 数据处理异常，设备：{device.Name}，标记：{tag.Name}, 地址：{tag.Address}";
                        _logger.LogError(ex, msg);
                        await _publisher.Publish(LoggingMessageEvent.Error(msg), PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);
                    }
                }
            });
        }

        return Task.CompletedTask;
    }

    private Task NoticeMonitorAsync(IDriverConnector connector)
    {
        var (channelName, device) = _deviceManager.GetDevice2(connector.Id);
        var tags = device!.GetAllTags(TagFlag.Notice);
        foreach (var tag in tags)
        {
            _ = Task.Run(async () =>
            {
                int pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _opsConfig.DefaultScanRate;
                while (_cts != null && !_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(pollingInterval, _cts.Token).ConfigureAwait(false);

                        if (_cts == null)
                        {
                            break;
                        }

                        if (!connector.CanConnect)
                        {
                            continue;
                        }

                        // 若读取失败，该信号点不会复位，下次会继续读取执行。
                        var (ok, data, err) = await connector.ReadAsync(tag).ConfigureAwait(false);
                        if (!ok)
                        {
                            string msg = $"[Engine] Notice 数据读取异常，设备：{device.Name}，标记：{tag.Name}, 地址：{tag.Address}，错误：{err}";
                            _logger.LogError(msg);
                            await _publisher.Publish(LoggingMessageEvent.Error(msg), PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);

                            continue;
                        }

                        // 在仅数据变更才会发送模式下，会校验数据是否有跳变。
                        if (tag.PublishMode == PublishMode.OnlyDataChanged && TagValueSet.CompareAndSwap(tag.TagId, data!.Value))
                        {
                            continue;
                        }

                        // 发布通知事件
                        await _publisher.Publish(new NoticeEvent
                        {
                            ChannelName = channelName,
                            Device = device,
                            Tag = tag,
                            Self = data,
                        }).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        string msg = $"[Engine] Notice 数据处理异常，设备：{device.Name}，标记：{tag.Name}, 地址：{tag.Address}";
                        _logger.LogError(ex, msg);
                        await _publisher.Publish(LoggingMessageEvent.Error(msg), PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);
                    }
                }
            });
        }

        return Task.CompletedTask;
    }

    private Task SwitchMonitorAsync(IDriverConnector connector)
    {
        var (channelName, device) = _deviceManager.GetDevice2(connector.Id);
        var tags = device!.GetAllTags(TagFlag.Switch);
        foreach (var tag in tags)
        {
            AsyncManualResetEvent mre = new(false); // 手动事件
           
            // 开关绑定的数据
            _ = Task.Run(async () =>
            {
                int pollingInterval = _opsConfig.SwitchScanRate > 0 ? _opsConfig.SwitchScanRate : 70;
                while (_cts != null && !_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await mre.WaitAsync().ConfigureAwait(false);

                        // 第一次检测
                        if (_cts == null)
                        {
                            break;
                        }

                        await Task.Delay(pollingInterval, _cts.Token).ConfigureAwait(false);

                        // 第二次检测
                        if (_cts == null)
                        {
                            break;
                        }

                        if (!connector.CanConnect)
                        {
                            continue;
                        }

                        // 记录数据
                        await _publisher.Publish(new SwitchEvent { Connector = connector, ChannelName = channelName, Device = device, Tag = tag }).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        string msg = $"[Engine] Switch 数据读取异常，设备：{device.Name}，标记：{tag.Name}, 地址：{tag.Address}";
                        _logger.LogError(ex, msg);
                        await _publisher.Publish(LoggingMessageEvent.Error(msg), PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);
                    }
                }
            });

            // 开关状态监控
            _ = Task.Run(async () =>
            {
                int pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _opsConfig.DefaultScanRate;
                bool isOn = false; // 开关处于的状态
                while (_cts != null && !_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(pollingInterval, _cts.Token).ConfigureAwait(false);

                        if (_cts == null)
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
                                    await _publisher.Publish(new SwitchEvent
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
                                    await _publisher.Publish(new SwitchEvent
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
                            await _publisher.Publish(new SwitchEvent 
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
                        string msg = $"[Engine] Switch 开关数据处理异常，设备：{device.Name}，标记：{tag.Name}, 地址：{tag.Address}";
                        _logger.LogError(ex, msg);
                        await _publisher.Publish(LoggingMessageEvent.Error(msg), PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);
                    }
                }

                // 任务取消后，无论什么情况都发送信号，确保让子任务也能退出
                mre.Set();
            });
        }

        return Task.CompletedTask;

      
    }

    public async Task ShutdownAsync()
    {
        string msg = "[Engine] 引擎已停止";
        if (!IsRunning)
        {
            await _publisher.Publish(LoggingMessageEvent.Info(msg)).ConfigureAwait(false);
            return;
        }
        IsRunning = false;

        CancellationTokenSource? cts = _cts;
        if (_cts != null)
        {
            _cts = null;
            cts!.Cancel();
        }

        // 需延迟 Dispose
        _ = Task.Run(async() =>
        {
            await Task.Delay(1000).ConfigureAwait(false);
            
            cts?.Dispose();
            _driverConnectorManager.Close();
        }).ConfigureAwait(false);

        _logger.LogInformation(msg);
        await _publisher.Publish(LoggingMessageEvent.Info(msg)).ConfigureAwait(false);
    }

    public void Dispose()
    {
        ShutdownAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 检查数据是否处于 On 状态。
    /// </summary>
    /// <remarks>数据类型必须为 bool 或 short 类型，且不为数组。</remarks>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    private static bool CheckOn(PayloadData data)
    {
        if (data.IsArray())
        {
            throw new NotSupportedException("Tag 数据类型不能为数组");
        }

        return data.DataType switch
        {
            TagDataType.Bit => data!.GetBit(),
            TagDataType.Int => data!.GetInt() == 1,
            _ => throw new NotSupportedException("Tag 数据类型必须为 bool 或 short。"),
        };
    }

    /// <summary>
    /// 设置标记为 Off 状态，bool 类型设置为 false, 数值类型设置为 0。
    /// </summary>
    /// <remarks>数据类型必须为 bool 或 short 类型，且不为数组。</remarks>
    /// <param name="tag"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    private static PayloadData SetOff(Tag tag)
    {
        object obj = SetOff2(tag);
        var data = PayloadData.FromTag(tag);
        data.Value = obj;
        return data;
    }

    /// <summary>
    /// 设置标记为 Off 状态，bool 类型设置为 false, 数值类型设置为 0。
    /// </summary>
    /// <remarks>数据类型必须为 bool 或 short 类型，且不为数组。</remarks>
    /// <param name="tag"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    private static object SetOff2(Tag tag)
    {
        if (tag.IsArray())
        {
            throw new NotSupportedException("Tag 数据类型不能为数组");
        }

        return tag.DataType switch
        {
            TagDataType.Bit => false,
            TagDataType.Int => (short)0,
            _ => throw new NotSupportedException("Tag 数据类型必须为 bool 或 short。"),
        };
    }
}
