using ThingsEdge.Contracts.Devices;
using ThingsEdge.Providers.Ops.Handlers;
using ThingsEdge.Router;

namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// 信息采集引擎。
/// </summary>
public sealed class OpsExchange : IExchange
{
    private CancellationTokenSource? _cts = new();

    private readonly IMediator _mediator;
    private readonly IDeviceManager _deviceManager;
    private readonly DriverConnectorManager _driverConnectorManager;
    private readonly OpsConfig _opsConfig;
    private readonly ILogger _logger;

    public OpsExchange(IMediator mediator,
        IDeviceManager deviceManager,
        DriverConnectorManager driverConnectorManager,
        IOptionsMonitor<OpsConfig> opsConfig,
        ILogger<OpsExchange> logger)
    {
        _mediator = mediator;
        _deviceManager = deviceManager;
        _driverConnectorManager = driverConnectorManager;
        _opsConfig = opsConfig.CurrentValue;
        _logger = logger;
    }

    /// <summary>
    /// 获取运行状态，是否正在运行中。
    /// </summary>
    public bool IsRuning { get; private set; }

    public async Task StartAsync()
    {
        if (IsRuning)
        {
            return;
        }
        IsRuning = true;

        _logger.LogInformation("[Engine] 引擎启动");

        _cts ??= new();

        var devices = _deviceManager.GetDevices();
        _driverConnectorManager.Load(devices);
        await _driverConnectorManager.ConnectAsync();

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

    private Task HeartbeatMonitorAsync(DriverConnector connector)
    {
        var device = _deviceManager.GetDevice(connector.Id);
        var tag = device!.Tags.FirstOrDefault(s => s.Flag == TagFlag.Heartbeat);
        if (tag == null)
        {
            return Task.CompletedTask;
        }

        _ = Task.Run(async () =>
        {
            int pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _opsConfig.DefaultScanRate;
            while (_cts != null && !_cts.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(pollingInterval, _cts.Token);

                    if (_cts == null)
                    {
                        break;
                    }

                    if (!connector.CanConnect)
                    {
                        continue;
                    }

                    // 心跳标记数据类型必须为 bool 或 int16
                    if (tag.DataType == DataType.Bit)
                    {
                        var result = await connector.Driver.ReadBoolAsync(tag.Address);
                        if (!result.IsSuccess)
                        {
                            _logger.LogError("[Engine] Heartbeat 数据读取异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}，错误：{Message}",
                                device.Name, tag.Name, tag.Address, result.Message);

                            continue;
                        }

                        if (result.Content)
                        {
                            await connector.Driver.WriteAsync(tag.Address, false);
                        }
                    }
                    else if (tag.DataType == DataType.Int)
                    {
                        var result = await connector.Driver.ReadInt16Async(tag.Address);
                        if (!result.IsSuccess)
                        {
                            _logger.LogError("[Engine] Heartbeat 数据读取异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}，错误：{Message}",
                                device.Name, tag.Name, tag.Address, result.Message);

                            continue;
                        }

                        if (result.Content == 1)
                        {
                            await connector.Driver.WriteAsync(tag.Address, (short)0);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Engine] Heartbeat 数据处理异常，设备：{device.Name}，变量：{tag.Name}, 地址：{tag.Address}",
                        device.Name, tag.Name, tag.Address);
                }
            }
        });

        return Task.CompletedTask;
    }

    private Task TriggerMonitorAsync(DriverConnector connector)
    {
        var device = _deviceManager.GetDevice(connector.Id);
        var tags = device!.GetTagsFromGroups(TagFlag.Trigger);
        foreach (var tag in tags)
        {
            _ = Task.Run(async () =>
            {
                int pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _opsConfig.DefaultScanRate;
                while (_cts != null && !_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(pollingInterval, _cts.Token);

                        if (_cts == null)
                        {
                            break;
                        }

                        var cts1 = _cts;

                        if (!connector.CanConnect)
                        {
                            continue;
                        }

                        // 若读取失败，该信号点不会复位，下次会继续读取执行。
                        var (ok, data, err) = await connector.ReadAsync(tag); // short 类型
                        if (!ok)
                        {
                            _logger.LogError("[Engine] Trigger 数据读取异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}，错误：{Err}",
                                device.Name, tag.Name, tag.Address, err);

                            continue;
                        }

                        // 校验触发标记
                        var state = data.GetInt();

                        // 检测标记状态是否有变动
                        if (!ValueCacheFactory.CompareAndSwap(tag.TagId, state))
                        {
                            // 推送数据
                            if (state == 1)
                            {
                                await _mediator.Publish(new TriggerEvent { Connector = connector, Device = device, Tag = tag, Self = data }, cts1.Token);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[Engine] Trigger 数据处理异常，设备：{DeviceName}，变量：{TagName}, 地址：{TagAddress}", 
                            device.Name, tag.Name, tag.Address);
                    }
                }
            });
        }

        return Task.CompletedTask;
    }

    private Task NoticeMonitorAsync(DriverConnector connector)
    {
        var device = _deviceManager.GetDevice(connector.Id);
        var tags = device!.GetTagsFromGroups(TagFlag.Notice);
        foreach (var tag in tags)
        {
            _ = Task.Run(async () =>
            {
                int pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _opsConfig.DefaultScanRate;
                while (_cts != null && !_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(pollingInterval, _cts.Token);

                        if (_cts == null)
                        {
                            break;
                        }

                        var cts1 = _cts;

                        if (!connector.CanConnect)
                        {
                            continue;
                        }

                        // 若读取失败，该信号点不会复位，下次会继续读取执行。
                        var (ok, data, err) = await connector.ReadAsync(tag);
                        if (!ok)
                        {
                            _logger.LogError("[Engine] Notice 数据读取异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}，错误：{Err}", 
                                device.Name, tag.Name, tag.Address, err);

                            continue;
                        }

                        // 推送数据
                        await _mediator.Publish(new NoticeEvent { Connector = connector, Device = device, Tag = tag, Self = data }, cts1.Token);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[Engine] Notice 数据处理异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}", 
                            device.Name, tag.Name, tag.Address);
                    }
                }
            });
        }

        return Task.CompletedTask;
    }

    private Task SwitchMonitorAsync(DriverConnector connector)
    {
        var device = _deviceManager.GetDevice(connector.Id);
        var tags = device!.GetTagsFromGroups(TagFlag.Switch);
        foreach (var tag in tags)
        {
            ManualResetEvent mre = new(false); // 手动事件
           
            // 开关绑定的数据
            _ = Task.Run(async () =>
            {
                int pollingInterval = _opsConfig.DefaultSwitchScanRate > 0 ? _opsConfig.DefaultSwitchScanRate : 30;
                while (_cts != null && !_cts.Token.IsCancellationRequested && mre.WaitOne())
                {
                    try
                    {
                        await Task.Delay(pollingInterval, _cts.Token);

                        if (_cts == null)
                        {
                            break;
                        }

                        var cts1 = _cts;

                        if (!connector.CanConnect)
                        {
                            continue;
                        }

                        // 记录数据
                        await _mediator.Publish(new SwitchEvent { Connector = connector, Device = device, Tag = tag }, cts1.Token);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[Engine] Switch 数据读取异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}",
                            device.Name, tag.Name, tag.Address);
                    }
                }
            });

            // 开关状态监控
            _ = Task.Run(async () =>
            {
                int pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _opsConfig.DefaultScanRate;
                DateTime lastestTime = DateTime.Now;
                bool isOn = false; // 开关开启状态
                while (_cts != null && !_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(pollingInterval, _cts.Token);

                        if (_cts == null)
                        {
                            break;
                        }

                        var cts1 = _cts;

                        if (!connector.CanConnect)
                        {
                            continue;
                        }

                        // 处于 off 状态，刷新最近一次进入的时间
                        if (!isOn)
                        {
                            lastestTime = DateTime.Now;
                        }

                        var (ok, data, _) = await connector.ReadAsync(tag);

                        // 开关标记数据类型必须为 bool 或 int16
                        bool on = tag.DataType switch
                        {
                            DataType.Bit => data.GetBit(),
                            DataType.Int => data.GetInt() == 1,
                            _ => throw new NotSupportedException(),
                        };

                        // 读取成功且开关处于 on 状态，发送开启动作信号。
                        if (ok && on)
                        {
                            // 在本身处于关闭状态，才执行开启动作。
                            if (!isOn)
                            {
                                // 发送 On 信号结束标识
                                await _mediator.Publish(new SwitchEvent
                                {
                                    Connector = connector,
                                    Device = device,
                                    Tag = tag,
                                    State = SwitchState.On,
                                    IsSwitchSignal = true,
                                }, cts1.Token);

                                // 开关开启时，发送信号，让子任务执行。
                                isOn = true;
                                mre.Set();
                            }
                            else
                            {
                                // 考虑设备在工作中发送意外中断，信号没有切换到 off 状态的场景（设备没有主动切换）。
                                // 可设置超时时长，开关信号连续处于开启状态时间不能超过多久。
                                if ((DateTime.Now - lastestTime).TotalSeconds > _opsConfig.AllowedSwitchOnlineMaxSeconds)
                                {
                                    // 发送 Off 信号结束标识
                                    await _mediator.Publish(new SwitchEvent
                                    {
                                        Connector = connector,
                                        Device = device,
                                        Tag = tag,
                                        State = SwitchState.Off,
                                        IsSwitchSignal = true,
                                    }, cts1.Token);

                                    // 运行超时，重置信号，让子任务阻塞。
                                    isOn = false;
                                    mre.Reset();
                                }
                            }

                            // 跳转
                            continue;
                        }

                        // 若读取失败，或是开关处于 off 状态，则发送关闭动作信号（防止因设备未掉线，而读取失败导致一直发送数据）。
                        if (isOn)
                        {
                            // 发送 Off 信号结束标识
                            await _mediator.Publish(new SwitchEvent 
                            {
                                Connector = connector,
                                Device = device,
                                Tag = tag,
                                State = SwitchState.Off,
                                IsSwitchSignal = true,
                            }, cts1.Token);

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
                        _logger.LogError(ex, "[Engine] Switch 开关数据处理异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}",
                            device.Name, tag.Name, tag.Address);
                    }
                }

                // 任务取消后，无论什么情况都发送信号，确保让子任务也能退出
                mre.Set();

                // 考虑如何安全调用 mre.Dispose()
                await Task.Delay(10);
                mre.Dispose();
            });
        }

        return Task.CompletedTask;
    }

    public async Task ShutdownAsync()
    {
        if (!IsRuning)
        {
            return;
        }
        IsRuning = false;

        if (_cts != null)
        {
            CancellationTokenSource cts = _cts;
            _cts = null;
            cts.Cancel();
            cts.Dispose();
        }

        await Task.Delay(500); // 阻塞 500ms
        _driverConnectorManager.Close();

        _logger.LogInformation("[Engine] 引擎停止");
    }

    public void Dispose()
    {
        ShutdownAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }
}
