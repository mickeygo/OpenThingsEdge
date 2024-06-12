using ThingsEdge.Providers.Ops.Events;
using ThingsEdge.Providers.Ops.Exchange.Monitors;
using ThingsEdge.Router;
using ThingsEdge.Router.Devices;

namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// 数据交换引擎。
/// </summary>
internal sealed class OpsExchange : IExchange, ISingletonDependency
{
    private CancellationTokenSource? _cts;

    private readonly IProducer _producer;
    private readonly IDeviceFactory _deviceFactory;
    private readonly DriverConnectorManager _driverConnectorManager;
    private readonly MonitorLoop _monitorLoop;
    private readonly ILogger _logger;

    public OpsExchange(IProducer producer,
        IDeviceFactory deviceFactory,
        DriverConnectorManager driverConnectorManager,
        MonitorLoop monitorLoop,
        ILogger<OpsExchange> logger)
    {
        _producer = producer;
        _deviceFactory = deviceFactory;
        _driverConnectorManager = driverConnectorManager;
        _monitorLoop = monitorLoop;
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
        await _producer.ProduceAsync(new ExchangeChangedEvent(RunningState.Startup)).ConfigureAwait(false);

        _cts = new();

        var devices = _deviceFactory.ReloadDevices();
        _driverConnectorManager.Load(devices);
        await _driverConnectorManager.ConnectAsync().ConfigureAwait(false);

        // 获取所有驱动，并监控设备数据
        foreach (var connector in _driverConnectorManager.GetAllDriver())
        {
            var (channelName, device) = _deviceFactory.GetDevice2(connector.Id);
            _monitorLoop.Monitor(connector, channelName!, device!, _cts.Token);
        }
    }

    public async Task ShutdownAsync()
    {
        if (!IsRunning)
        {
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
        await Task.Delay(1000).ConfigureAwait(false);

        //cts?.Dispose(); // Dispose 会导致部分问题
        _driverConnectorManager.Close();

        _logger.LogInformation("[Engine] 引擎已停止");
        await _producer.ProduceAsync(new ExchangeChangedEvent(RunningState.Stop)).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await ShutdownAsync().ConfigureAwait(false);
    }
}
