namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// 信息采集引擎。
/// </summary>
public sealed class OpsEngine : IOpsEngine
{
    private CancellationTokenSource? _cts = new();

    private readonly IDeviceManager _deviceManager;
    private readonly DriverConnectorManager _driverConnectorManager;
    private readonly IMessagePusher _messagePusher;
    private readonly OpsConfig _opsConfig;
    private readonly ILogger _logger;

    public OpsEngine(IDeviceManager deviceManager,
        DriverConnectorManager driverConnectorManager,
        IMessagePusher messagePusher,
        IOptionsMonitor<OpsConfig> opsConfig,
        ILogger<OpsEngine> logger)
    {
        _deviceManager = deviceManager;
        _messagePusher = messagePusher;
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

                    if (_cts != null)
                    {
                        break;
                    }

                    if (!connector.CanConnect)
                    {
                        continue;
                    }

                    var result = await connector.Driver.ReadBoolAsync(tag.Address);
                    if (!result.IsSuccess)
                    {
                        _logger.LogError($"[Engine] Heartbeat 数据读取异常，设备：{device.Name}，标记：{tag.Name}, 地址：{tag.Address}，错误：{result.Message}");

                        continue;
                    }

                    if (result.Content)
                    {
                        await connector.Driver.WriteAsync(tag.Address, false);
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[Engine] Heartbeat 数据处理异常，设备：{device.Name}，变量：{tag.Name}, 地址：{tag.Address}");
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

                        if (!connector.CanConnect)
                        {
                            continue;
                        }

                        // 若读取失败，该信号点不会复位，下次会继续读取执行。
                        var (ok, data, err) = await connector.ReadAsync(tag); // short 类型
                        if (!ok)
                        {
                            _logger.LogError($"[Engine] Trigger 数据读取异常，设备：{device.Name}，标记：{tag.Name}, 地址：{tag.Address}，错误：{err}");

                            continue;
                        }

                        // 校验触发标记
                        var state = data.GetInt();
                        if (state == 1)
                        {
                            // 检测标记状态是否有变动
                            if (ValueCacheFactory.CompareAndSwap(tag.TagId, state))
                            {
                                continue;
                            }

                            // 推送数据
                            await _messagePusher.PushAsync(connector, device, tag, data, _cts.Token);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[Engine] Trigger 数据处理异常，设备：{device.Name}，变量：{tag.Name}, 地址：{tag.Address}");
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

                        if (!connector.CanConnect)
                        {
                            continue;
                        }

                        // 若读取失败，该信号点不会复位，下次会继续读取执行。
                        var (ok, data, err) = await connector.ReadAsync(tag);
                        if (!ok)
                        {
                            _logger.LogError($"[Engine] Notice 数据读取异常，设备：{device.Name}，标记：{tag.Name}, 地址：{tag.Address}，错误：{err}");

                            continue;
                        }

                        // 推送数据
                        await _messagePusher.PushAsync(connector, device, tag, data, _cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[Engine] Notice 数据处理异常，设备：{device.Name}，标记：{tag.Name}, 地址：{tag.Address}");
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
            object syncLock = new();
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

                        if (!connector.CanConnect)
                        {
                            continue;
                        }

                        // 推送数据
                        await _messagePusher.PushAsync(connector, device, tag, null, _cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[Engine] Switch 数据读取异常，设备：{device.Name}，标记：{tag.Name}, 地址：{tag.Address}");
                    }
                }
            });

            // 开关状态监控
            _ = Task.Run(async () =>
            {
                int pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _opsConfig.DefaultScanRate;
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

                        if (!connector.CanConnect)
                        {
                            continue;
                        }

                        // 若读取失败，该信号点不会复位，下次会继续读取执行。
                        var (ok, data, _) = await connector.ReadAsync(tag);
                        if (ok)
                        {
                            if (data.GetBit())
                            {
                                // On信号，若本身处于关闭状态，则执行开启动作。
                                if (!isOn)
                                {
                                    if (_cts != null)
                                    {
                                        // TODO: 发送 On 信号

                                    }

                                    // 开关开启时，发送信号，让子任务执行。
                                    mre.Set();
                                    isOn = true;
                                }
                            }
                            else
                            {
                                // Off信号，若本身处于开启状态，则执行关闭动作。
                                if (isOn)
                                {
                                    if (_cts != null)
                                    {
                                        // TODO: 发送 Off 信号

                                    }

                                    // 读取失败或开关关闭时，重置信号，让子任务阻塞。
                                    mre.Reset();
                                    isOn = false;
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[Engine] Switch 开关数据处理异常，设备：{device.Name}，标记：{tag.Name}, 地址：{tag.Address}");
                    }
                }

                // 任务取消后，无论什么情况都发送信号，确保让子任务也能退出
                mre.Set();

                // TODO: 考虑如何安全调用 mre.Dispose()
            });
        }

        return Task.CompletedTask;
    }

    public void Stop()
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

        Task.Delay(500).ConfigureAwait(false).GetAwaiter().GetResult(); // 阻塞 500ms
        _driverConnectorManager.Close();

        _logger.LogInformation("[Engine] 引擎停止");
    }

    public void Dispose()
    {
        Stop();
    }
}
