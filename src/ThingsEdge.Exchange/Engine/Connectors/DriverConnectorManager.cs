using ThingsEdge.Communication;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.ModBus;
using ThingsEdge.Communication.Profinet.AllenBradley;
using ThingsEdge.Communication.Profinet.Delta;
using ThingsEdge.Communication.Profinet.Fuji;
using ThingsEdge.Communication.Profinet.Inovance;
using ThingsEdge.Communication.Profinet.Melsec;
using ThingsEdge.Communication.Profinet.Omron;
using ThingsEdge.Communication.Profinet.Panasonic;
using ThingsEdge.Communication.Profinet.Siemens;
using ThingsEdge.Communication.Profinet.XINJE;
using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Engine.Connectors;

/// <summary>
/// 驱动连接器管理者。
/// </summary>
internal sealed class DriverConnectorManager(ILogger<DriverConnectorManager> logger) : IDisposable
{
    private readonly Dictionary<string, IDriverConnector> _connectors = []; // Key 为设备编号

    private bool _hasTryConnectServer;
    private PeriodicTimer? _periodicTimer;

    private object SyncLock => _connectors;

    /// <summary>
    /// 获取指定的连接驱动
    /// </summary>
    /// <param name="deviceId">设备Id</param>
    /// <returns></returns>
    public IDriverConnector this[string deviceId] => _connectors[deviceId];

    /// <summary>
    /// 获取指定的连接驱动
    /// </summary>
    /// <param name="deviceId">设备Id</param>
    /// <returns></returns>
    public IDriverConnector? GetConnector(string deviceId)
    {
        if (_connectors.TryGetValue(deviceId, out var connector))
        {
            return connector;
        }
        return default;
    }

    /// <summary>
    /// 获取所有的连接驱动
    /// </summary>
    /// <returns></returns>
    public IReadOnlyCollection<IDriverConnector> GetAllDriver()
    {
        return _connectors.Values;
    }

    /// <summary>
    /// 加载驱动。
    /// </summary>
    /// <param name="deviceInfos"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Load(IEnumerable<Device> deviceInfos)
    {
        foreach (var deviceInfo in deviceInfos)
        {
            DeviceTcpNet driverNet = deviceInfo.Model switch
            {
                DriverModel.ModbusTcp => new ModbusTcpNet(deviceInfo.Host),
                DriverModel.S7_1500 => new SiemensS7Net(SiemensPLCS.S1500, deviceInfo.Host),
                DriverModel.S7_1200 => new SiemensS7Net(SiemensPLCS.S1200, deviceInfo.Host),
                DriverModel.S7_400 => new SiemensS7Net(SiemensPLCS.S400, deviceInfo.Host),
                DriverModel.S7_300 => new SiemensS7Net(SiemensPLCS.S300, deviceInfo.Host),
                DriverModel.S7_S200 => new SiemensS7Net(SiemensPLCS.S200, deviceInfo.Host),
                DriverModel.S7_S200Smart => new SiemensS7Net(SiemensPLCS.S200Smart, deviceInfo.Host),
                DriverModel.Melsec_MC => new MelsecMcNet(deviceInfo.Host, deviceInfo.Port),
                DriverModel.Melsec_MCAscii => new MelsecMcAsciiNet(deviceInfo.Host, deviceInfo.Port),
                DriverModel.Melsec_MCR => new MelsecMcRNet(deviceInfo.Host, deviceInfo.Port),
                DriverModel.Melsec_A1E => new MelsecA1ENet(deviceInfo.Host, deviceInfo.Port),
                DriverModel.Melsec_CIP => new MelsecCipNet(deviceInfo.Host),
                DriverModel.Omron_FinsTcp => new OmronFinsNet(deviceInfo.Host, deviceInfo.Port),
                DriverModel.Omron_CIP => new OmronCipNet(deviceInfo.Host, deviceInfo.Port),
                DriverModel.Omron_HostLinkOverTcp => new OmronHostLinkOverTcp(deviceInfo.Host, deviceInfo.Port),
                DriverModel.Omron_HostLinkCModeOverTcp => new OmronHostLinkCModeOverTcp(deviceInfo.Host, deviceInfo.Port),
                DriverModel.AllenBradley_CIP => new AllenBradleyNet(deviceInfo.Host),
                DriverModel.Inovance_Tcp => new InovanceTcpNet(deviceInfo.Host),
                DriverModel.Delta_Tcp => new DeltaTcpNet(deviceInfo.Host),
                DriverModel.Fuji_SPH => new FujiSPHNet(deviceInfo.Host),
                DriverModel.Panasonic_Mc => new PanasonicMcNet(deviceInfo.Host, deviceInfo.Port),
                DriverModel.XinJE_Tcp => new XinJETcpNet(deviceInfo.Host),
                _ => throw new NotImplementedException(),
            };

            // 设置 SocketKeepAliveTime 心跳时间
            driverNet.KeepAliveTime = 60_000;
            _connectors.Add(deviceInfo.DeviceId, new DriverConnector(deviceInfo.DeviceId, deviceInfo.Host, deviceInfo.Port, driverNet, deviceInfo.MaxPDUSize));
        }
    }

    /// <summary>
    /// 驱动连接到服务
    /// </summary>
    /// <returns></returns>
    public async Task ConnectAsync()
    {
        if (!_hasTryConnectServer)
        {
            foreach (var connector in _connectors.Values)
            {
                if (connector.Driver is DeviceTcpNet networkDevice)
                {
                    connector.ConnectedStatus = ConnectionStatus.Disconnected; // 初始化

                    // 回调，在长连接异常关闭后设置连接状态为 Disconnected。
                    networkDevice.SocketErrorClosedDelegate = code =>
                    {
                        // 根据错误代码来判断是否断开连接
                        if (networkDevice.IsSocketError)
                        {
                            connector.ConnectedStatus = ConnectionStatus.Disconnected;

                            if (code is (int)CommErrorCode.SocketConnectException
                                    or (int)CommErrorCode.SocketConnectTimeoutException
                                    or (int)CommErrorCode.RemoteClosedConnection
                                    or (int)CommErrorCode.ReceiveDataTimeout
                                    or (int)CommErrorCode.SocketSendException
                                    or (int)CommErrorCode.SocketReceiveException
                                    or (int)CommErrorCode.SocketException)
                            {
                                logger.LogWarning("已与服务器断开，主机：{Host}，错误代码：{Code}", connector.Host, code);
                            }
                        }
                    };

                    // 先检查服务器能否访问
                    try
                    {
                        if (await networkDevice.PingSuccessfulAsync(1_000).ConfigureAwait(false))
                        {
                            connector.Available = true;
                            var ret = await networkDevice.ConnectServerAsync().ConfigureAwait(false);
                            if (ret.IsSuccess)
                            {
                                connector.ConnectedStatus = ConnectionStatus.Connected;
                            }
                            else
                            {
                                logger.LogWarning("尝试连接服务失败，错误：{Message}，主机：{Host}，端口：{Port}", ret.Message, connector.Host, connector.Port);
                            }
                        }
                        else
                        {
                            logger.LogWarning("尝试 Ping 服务失败，主机：{Host}", connector.Host);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "尝试连接服务异常，主机：{Host}，端口：{Port}", connector.Host, connector.Port);
                    }
                }
            }

            _hasTryConnectServer = true;

            // 开启心跳检测
            // 采用 PeriodicTimer 而不是普通的 Timer 定时器，是为了防止产生任务重叠执行。
            _ = PeriodicHeartbeat();
        }
    }

    private Task PeriodicHeartbeat()
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(3000).ConfigureAwait(false); // 延迟3s后开始监听

            HashSet<string> pingSuccessHosts = []; // 存放已 Ping 成功的主机信息。

            // PeriodicTimer 定时器，可以让任务不堆积，不会因上一个任务阻塞在下个任务开始时导致多个任务同时进行。
            _periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(2));
            while (await _periodicTimer.WaitForNextTickAsync().ConfigureAwait(false))
            {
                foreach (var connector in _connectors.Values)
                {
                    // 若连接状态处于断开状态，网络检查 OK 后会进行重连。
                    // 对于初始时设备不可用，后续可用的情况下会自动进行连接。
                    if (connector.Driver is DeviceTcpNet networkDevice)
                    {
                        try
                        {
                            // 若连接器 Host 相同，每次轮询只需要 Ping 一次即可
                            if (pingSuccessHosts.Contains(connector.Host))
                            {
                                connector.Available = true;
                            }
                            else
                            {
                                connector.Available = await networkDevice.PingSuccessfulAsync(1_000).ConfigureAwait(false);
                                if (connector.Available)
                                {
                                    pingSuccessHosts.Add(connector.Host);
                                }
                            }

                            // 注： networkDevice 中连接成功一次，即使服务器断开一段时间后再恢复，连接依旧可用，
                            // 所以，在连接成功一次后，不要再重复连接。
                            if (connector.Available && connector.ConnectedStatus == ConnectionStatus.Disconnected)
                            {
                                // 内部 Socket 异常，或是第一次尝试连接过服务器失败
                                if (networkDevice.IsSocketError)
                                {
                                    var result = await networkDevice.ConnectServerAsync().ConfigureAwait(false);
                                    if (result.IsSuccess)
                                    {
                                        connector.ConnectedStatus = ConnectionStatus.Connected;
                                        logger.LogInformation("已连接上服务，主机：{Host}", connector.Host);
                                    }
                                }
                            }
                        }
                        catch (PingException ex)
                        {
                            logger.LogWarning("尝试 Ping 服务器异常，主机：{Host}，异常：{Message}", connector.Host, ex.Message);
                        }
                        catch (Exception ex)
                        {
                            connector.Available = false;
                            logger.LogError(ex, "[DriverConnectorManager] Ping 驱动服务器出现异常，主机：{Host}。", connector.Host);
                        }
                    }
                }

                // 一次循环结束后，清空已 Ping 的主机
                pingSuccessHosts.Clear();
            }
        });

        return Task.CompletedTask;
    }

    /// <summary>
    /// 驱动连接挂起
    /// </summary>
    public void Suspend()
    {
        lock (SyncLock)
        {
            foreach (var connector in _connectors.Values)
            {
                if (connector.DriverStatus == DriverStatus.Normal)
                {
                    connector.DriverStatus = DriverStatus.Suspended;
                }
            }
        }
    }

    /// <summary>
    /// 重启驱动状态。
    /// </summary>
    public void Restart()
    {
        lock (SyncLock)
        {
            foreach (var connector in _connectors.Values)
            {
                if (connector.DriverStatus == DriverStatus.Suspended)
                {
                    connector.DriverStatus = DriverStatus.Normal;
                }
            }
        }
    }

    /// <summary>
    /// 关闭并释放所有连接，同时会清空连接缓存。
    /// </summary>
    public void Close()
    {
        lock (SyncLock)
        {
            if (_hasTryConnectServer)
            {
                foreach (var connector in _connectors.Values)
                {
                    connector.ConnectedStatus = ConnectionStatus.Aborted;
                    if (connector.Driver is DeviceTcpNet networkDevice)
                    {
                        networkDevice.Dispose();
                    }
                }

                _periodicTimer?.Dispose();
                _connectors.Clear();
                _hasTryConnectServer = false;
            }
        }
    }

    public void Dispose()
    {
        Close();
    }
}
