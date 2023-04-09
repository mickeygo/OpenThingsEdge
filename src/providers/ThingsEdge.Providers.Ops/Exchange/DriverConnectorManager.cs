using Ops.Communication.Core.Net;
using Ops.Communication.Modbus;
using Ops.Communication.Profinet.AllenBradley;
using Ops.Communication.Profinet.Melsec;
using Ops.Communication.Profinet.Omron;
using Ops.Communication.Profinet.Siemens;

namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// 驱动连接器管理者。
/// </summary>
public sealed class DriverConnectorManager : IDisposable
{
    private readonly Dictionary<string, DriverConnector> _connectors = new(); // Key 为设备编号
    private bool _isConnectedServer;
    private Timer? _heartbeatTimer;
    private readonly ILogger _logger;

    private object SyncLock => _connectors;

    public DriverConnectorManager(ILogger<DriverConnectorManager> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 获取指定的连接驱动
    /// </summary>
    /// <param name="name">设备名称</param>
    /// <returns></returns>
    public DriverConnector this[string name] => _connectors[name];

    /// <summary>
    /// 获取指定的连接驱动
    /// </summary>
    /// <param name="name">设备名称</param>
    /// <returns></returns>
    public DriverConnector? GetConnector(string name)
    {
        if (_connectors.TryGetValue(name, out var connector))
        {
            return connector;
        }
        return default;
    }

    /// <summary>
    /// 获取所有的连接驱动
    /// </summary>
    /// <returns></returns>
    public IReadOnlyCollection<DriverConnector> GetAllDriver()
    {
        return _connectors.Values;
    }

    /// <summary>
    /// 加载驱动。
    /// </summary>
    /// <param name="deviceInfos"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Load(IEnumerable<DeviceInfo> deviceInfos)
    {
        foreach (var deviceInfo in deviceInfos)
        {
            NetworkDeviceBase driverNet = deviceInfo.Model switch
            {
                DeviceModel.ModbusTcp => new ModbusTcpNet(deviceInfo.Host),
                DeviceModel.S7_1500 => new SiemensS7Net(SiemensPLCS.S1500, deviceInfo.Host),
                DeviceModel.S7_1200 => new SiemensS7Net(SiemensPLCS.S1200, deviceInfo.Host),
                DeviceModel.S7_400 => new SiemensS7Net(SiemensPLCS.S400, deviceInfo.Host),
                DeviceModel.S7_300 => new SiemensS7Net(SiemensPLCS.S300, deviceInfo.Host),
                DeviceModel.S7_S200 => new SiemensS7Net(SiemensPLCS.S200, deviceInfo.Host),
                DeviceModel.S7_S200Smart => new SiemensS7Net(SiemensPLCS.S200Smart, deviceInfo.Host),
                DeviceModel.Melsec_CIP => new MelsecCipNet(deviceInfo.Host),
                DeviceModel.Melsec_A1E => new MelsecA1ENet(deviceInfo.Host, deviceInfo.Port),
                DeviceModel.Melsec_MC => new MelsecMcNet(deviceInfo.Host, deviceInfo.Port),
                DeviceModel.Melsec_MCR => new MelsecMcRNet(deviceInfo.Host, deviceInfo.Port),
                DeviceModel.Omron_FinsTcp => new OmronFinsNet(deviceInfo.Host, deviceInfo.Port),
                DeviceModel.Omron_CipNet => new OmronCipNet(deviceInfo.Host, deviceInfo.Port),
                DeviceModel.Omron_HostLinkOverTcp => new OmronHostLinkOverTcp(deviceInfo.Host, deviceInfo.Port),
                DeviceModel.Omron_HostLinkCModeOverTcp => new OmronHostLinkCModeOverTcp(deviceInfo.Host, deviceInfo.Port),
                DeviceModel.AllenBradley_CIP => new AllenBradleyNet(deviceInfo.Host),
                _ => throw new NotImplementedException(),
            };

            // 设置 SocketKeepAliveTime 心跳时间
            driverNet.SocketKeepAliveTime = 60_000;
            _connectors.Add(deviceInfo.Name, new DriverConnector(deviceInfo.Name, deviceInfo.Host, deviceInfo.Port, driverNet));
        }
    }

    /// <summary>
    /// 驱动连接到服务
    /// </summary>
    /// <returns></returns>
    public async Task ConnectAsync()
    {
        if (!_isConnectedServer)
        {
            foreach (var connector in _connectors.Values)
            {
                if (connector.Driver is NetworkDeviceBase networkDevice)
                {
                    networkDevice.SetPersistentConnection(); // 设置为长连接

                    // 注册方法，在每次连接成功或失败后重置连接状态。
                    networkDevice.ConnectServerPostDelegate = (ok) =>
                    {
                        connector.ConnectedStatus = ok ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;
                    };

                    // 先检查服务器能否访问
                    try
                    {
                        var ipStatus = await networkDevice.PingIpAddressAsync(1_000);
                        if (ipStatus == IPStatus.Success)
                        {
                            connector.Available = true;
                            _ = await networkDevice.ConnectServerAsync();
                        }
                        else
                        {
                            connector.ConnectedStatus = ConnectionStatus.Disconnected;
                        }
                    }
                    catch (Exception)
                    {
                        connector.ConnectedStatus = ConnectionStatus.Disconnected;
                    }
                }
            }

            _isConnectedServer = true;

            // 开启心跳检测
            var state = new WeakReference<DriverConnectorManager>(this);
            var period = _connectors.Count * 1_000;
            _heartbeatTimer = new Timer(Heartbeat, state, 1000, period); // 按设备数量设定监听时长
        }
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
            if (_isConnectedServer)
            {
                foreach (var connector in _connectors.Values)
                {
                    connector.ConnectedStatus = ConnectionStatus.Aborted;
                    if (connector.Driver is NetworkDeviceBase networkDevice)
                    {
                        networkDevice.Dispose();
                    }
                }

                _connectors.Clear();
                _heartbeatTimer?.Dispose();
                _isConnectedServer = false;
            }
        }
    }

    public void Dispose()
    {
        Close();
    }

    /// <summary>
    /// 轮询监听是否能访问服务器
    /// </summary>
    private void Heartbeat(object? state)
    {
        var weakReference = (WeakReference<DriverConnectorManager>)state!;
        if (weakReference.TryGetTarget(out var target))
        {
            target.Heartbeat2();
        }
    }

    private void Heartbeat2()
    {
        DriverConnector[] driverConnectors;
        lock (SyncLock)
        {
            driverConnectors = _connectors.Values.ToArray();
        }

        foreach (var connector in driverConnectors)
        {
            // 若连接状态处于断开状态，网络检查 OK 后会进行重连。
            // 对于初始时设备不可用，后续可用的情况下会自动进行连接。
            if (connector.Driver is NetworkDeviceBase networkDevice
                && connector.ConnectedStatus == ConnectionStatus.Disconnected)
            {
                try
                {
                    connector.Available = networkDevice.PingIpAddress(700) == IPStatus.Success;
                    if (connector.Available)
                    {
                        _ = networkDevice.ConnectServer();
                    }
                }
                catch (Exception ex)
                {
                    connector.Available = false;
                    _logger.LogError(ex, "[DriverConnectorManager] Ping 驱动服务器出现异常。");
                }
            }
        }
    }
}
