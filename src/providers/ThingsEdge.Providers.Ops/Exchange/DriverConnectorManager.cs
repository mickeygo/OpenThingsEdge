using Ops.Communication.Core.Net;

namespace ThingsEdge.Providers.Ops.Exchange;

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
    /// <param name="id">设备Id</param>
    /// <returns></returns>
    public DriverConnector this[string id] => _connectors[id];

    /// <summary>
    /// 获取指定的连接驱动
    /// </summary>
    /// <param name="id">设备Id</param>
    /// <returns></returns>
    public DriverConnector? GetConnector(string id)
    {
        if (_connectors.TryGetValue(id, out var connector))
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
            _heartbeatTimer = new Timer(Heartbeat, state, 1000, 2000); // 2s 监听一次能否 ping 通服务器
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
                    connector.Available = networkDevice.PingIpAddress() == IPStatus.Success;
                    if (connector.Available)
                    {
                        _ = networkDevice.ConnectServer();
                    }
                }
                catch (Exception)
                {
                    connector.Available = false;
                    //
                }
            }
        }
    }
}
