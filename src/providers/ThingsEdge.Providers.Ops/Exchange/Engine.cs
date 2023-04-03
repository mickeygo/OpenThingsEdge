using MediatR;
using Ops.Exchange.Monitors;

namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// 信息采集引擎。
/// </summary>
public sealed class Engine : IEngine
{
    private CancellationTokenSource? _cts = new();
    private readonly IMediator _mediator;
    private readonly DriverConnectorManager _driverConnectorManager;
    private readonly ILogger _logger;

    public Engine(IMediator mediator, DriverConnectorManager driverConnectorManager, ILogger<MonitorManager> logger)
    {
        _mediator = mediator;
        _driverConnectorManager = driverConnectorManager;
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


        throw new NotImplementedException();
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

        _logger.LogInformation("[Monitor] 监控停止");
    }

    public void Dispose()
    {
        Stop();
    }
}
