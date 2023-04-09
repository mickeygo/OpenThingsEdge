namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// 信息采集引擎。
/// </summary>
public sealed class Engine : IEngine
{
    private CancellationTokenSource? _cts = new();
    private readonly IDeviceManager _deviceManager;
    private readonly DriverConnectorManager _driverConnectorManager;
    private readonly IMessagePusher _messagePusher;
    private readonly OpsConfig _opsConfig;
    private readonly ILogger _logger;

    public Engine(IDeviceManager deviceManager,
        DriverConnectorManager driverConnectorManager,
        IMessagePusher messagePusher,
        IOptions<OpsConfig> opsConfig,
        ILogger<Engine> logger)
    {
        _deviceManager = deviceManager;
        _messagePusher = messagePusher;
        _driverConnectorManager = driverConnectorManager;
        _opsConfig = opsConfig.Value;
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

        var deviceInfos = await _deviceManager.GetAllAsync();
        _driverConnectorManager.Load(deviceInfos);
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

    private async Task HeartbeatMonitorAsync(DriverConnector connector)
    {
        var deviceInfo = await _deviceManager.GetAsync(connector.Id);
        var tag = deviceInfo!.Tags.FirstOrDefault(s => s.Flag == TagFlag.Heartbeat);
        if (tag == null)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            int pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _opsConfig.DefaultScanRate;
            while (_cts != null && !_cts.Token.IsCancellationRequested)
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

                try
                {
                    var result = await connector.Driver.ReadBoolAsync(tag.Address);
                    if (!result.IsSuccess)
                    {
                        _logger.LogError($"[Engine] Heartbeat 数据读取异常，设备：{deviceInfo.Name}，标记：{tag.Name}, 地址：{tag.Address}，错误：{result.Message}");

                        continue;
                    }

                    if (result.Content)
                    {
                        await connector.Driver.WriteAsync(tag.Address, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[Engine] Heartbeat 数据处理异常，设备：{deviceInfo.Name}，变量：{tag.Name}, 地址：{tag.Address}");
                }
            }
        });
    }

    private async Task TriggerMonitorAsync(DriverConnector connector)
    {
        var deviceInfo = await _deviceManager.GetAsync(connector.Id);
        var tags = deviceInfo!.GetTagsFromGroups(TagFlag.Trigger);
        foreach (var tag in tags)
        {
            _ = Task.Run(async () =>
            {
                int pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _opsConfig.DefaultScanRate;
                while (_cts != null && !_cts.Token.IsCancellationRequested)
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

                    try
                    {
                        // 若读取失败，该信号点不会复位，下次会继续读取执行。
                        var (ok, data, err) = await connector.ReadAsync(tag); // short 类型
                        if (!ok)
                        {
                            _logger.LogError($"[Engine] Trigger 数据读取异常，设备：{deviceInfo.Name}，标记：{tag.Name}, 地址：{tag.Address}，错误：{err}");

                            continue;
                        }

                        // 校验触发标记
                        var state = data.GetInt();
                        if (state == 1)
                        {
                            // 检测标记状态是否有变动
                            if (StateCacheFactory.CompareAndSwap(tag.Id.ToString(), state))
                            {
                                continue;
                            }

                            // 推送数据
                            await _messagePusher.PushAsync(connector, tag, data);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[Engine] Trigger 数据处理异常，设备：{deviceInfo.Name}，变量：{tag.Name}, 地址：{tag.Address}");
                    }
                }
            });
        }
    }

    private async Task NoticeMonitorAsync(DriverConnector connector)
    {
        var deviceInfo = await _deviceManager.GetAsync(connector.Id);
        var tags = deviceInfo!.GetTagsFromGroups(TagFlag.Notice);
        foreach (var tag in tags)
        {
            _ = Task.Run(async () =>
            {
                int pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _opsConfig.DefaultScanRate;
                while (_cts != null && !_cts.Token.IsCancellationRequested)
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

                    try
                    {
                        // 若读取失败，该信号点不会复位，下次会继续读取执行。
                        var (ok, data, err) = await connector.ReadAsync(tag);
                        if (!ok)
                        {
                            _logger.LogError($"[Engine] Notice 数据读取异常，设备：{deviceInfo.Name}，标记：{tag.Name}, 地址：{tag.Address}，错误：{err}");

                            continue;
                        }

                        // 推送数据
                        await _messagePusher.PushAsync(connector, tag, data);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[Engine] Notice 数据处理异常，设备：{deviceInfo.Name}，标记：{tag.Name}, 地址：{tag.Address}");
                    }
                }
            });
        }
    }

    private async Task SwitchMonitorAsync(DriverConnector connector)
    {
        var deviceInfo = await _deviceManager.GetAsync(connector.Id);
        var tags = deviceInfo!.GetTagsFromGroups(TagFlag.Switch);
        foreach (var tag in tags)
        {
            ManualResetEvent mre = new(false); // 手动事件

            // 开关绑定的数据
            _ = Task.Run(async () =>
            {
                int pollingInterval = _opsConfig.DefaultSwitchScanRate > 0 ? _opsConfig.DefaultSwitchScanRate : 30;
                while (mre.WaitOne() && _cts != null && !_cts.Token.IsCancellationRequested)
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

                    try
                    {
                        // 推送数据
                        await _messagePusher.PushAsync(connector, tag, null);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[Engine] Switch 数据处理异常，设备：{deviceInfo.Name}，标记：{tag.Name}, 地址：{tag.Address}");
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

                // 任务取消后，发送信号，让子任务能退出任务
                if (!isOn)
                {
                    mre.Set();
                    isOn = true;
                }
            });
        }
    }

    /// <summary>
    /// 停止运行。
    /// </summary>
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
