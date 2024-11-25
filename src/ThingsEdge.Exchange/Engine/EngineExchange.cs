using ThingsEdge.Exchange.Addresses;
using ThingsEdge.Exchange.Engine.Connectors;
using ThingsEdge.Exchange.Engine.Snapshot;
using ThingsEdge.Exchange.Interfaces;

namespace ThingsEdge.Exchange.Engine;

/// <summary>
/// 数据交换引擎。
/// </summary>
internal sealed class EngineExchange(IAddressFactory addressFactory,
    ITagDataSnapshot tagDataSnapshot,
    DriverConnectorManager driverConnectorManager,
    EngineExecutor engineExcutor,
    ILogger<EngineExchange> logger) : IExchange
{
    private CancellationTokenSource? _cts;

    public bool IsRunning { get; private set; }

    public async Task StartAsync()
    {
        if (IsRunning)
        {
            return;
        }
        IsRunning = true;

        logger.LogInformation("[EngineExchange] 引擎启动");

        _cts ??= new();
        var addresses = addressFactory.ReloadAddress();
        driverConnectorManager.Load(addresses);
        await driverConnectorManager.ConnectAsync().ConfigureAwait(false);

        // 获取所有驱动，并监控设备数据
        foreach (var connector in driverConnectorManager.GetAllDriver())
        {
            var (channelName, device) = addressFactory.GetDevice2(connector.Id);
            await engineExcutor.ExecuteAsync(connector, channelName!, device!, _cts.Token).ConfigureAwait(false);
        }
    }

    public async Task ShutdownAsync()
    {
        if (!IsRunning)
        {
            return;
        }
        IsRunning = false;

        // 清空缓存与快照
        TagDataCache.Clear();
        tagDataSnapshot.Clear();

        var cts = _cts;
        if (_cts != null)
        {
            _cts = null;
            cts!.Cancel();
        }

        // 需延迟 Dispose
        await Task.Delay(1000).ConfigureAwait(false);

        //cts?.Dispose(); // Dispose 会导致部分问题
        driverConnectorManager.Close();

        logger.LogInformation("[EngineExchange] 引擎已停止");
    }

    public async ValueTask DisposeAsync()
    {
        await ShutdownAsync().ConfigureAwait(false);
    }
}
