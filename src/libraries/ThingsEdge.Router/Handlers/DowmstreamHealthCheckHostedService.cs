﻿namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 下游服务健康检测后台服务。
/// </summary>
internal sealed class DowmstreamHealthCheckHostedService : IHostedService
{
    private readonly IDownstreamHealthChecker _downstreamHealthChecker;
    private readonly IHealthCheckHandlePolicy _healthCheckHandlePolicy;
    private readonly ILogger _logger;

    private readonly PeriodicTimer _timer;

    public DowmstreamHealthCheckHostedService(IDownstreamHealthChecker downstreamHealthChecker, 
        IHealthCheckHandlePolicy healthCheckHandlePolicy,
        ILogger<DowmstreamHealthCheckHostedService> logger)
    {
        _downstreamHealthChecker = downstreamHealthChecker;
        _healthCheckHandlePolicy = healthCheckHandlePolicy;
        _logger = logger;

        _timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (await _timer.WaitForNextTickAsync(cancellationToken))
        {
            try
            {
                var state = await _downstreamHealthChecker.CheckAsync(cancellationToken);
                await _healthCheckHandlePolicy.HandleAsync(state, cancellationToken);
            }
            catch
            {
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Dispose();
        return Task.CompletedTask;
    }
}
