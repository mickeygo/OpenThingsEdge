namespace ThingsEdge.Router.Handlers.Health;

/// <summary>
/// 目标服务健康检测后台服务。
/// </summary>
internal sealed class DestinationHealthCheckHostedService : IHostedService
{
    private readonly IDestinationHealthChecker _downstreamHealthChecker;
    private readonly IHealthCheckHandlePolicy _healthCheckHandlePolicy;

    private readonly PeriodicTimer _timer;

    public DestinationHealthCheckHostedService(IDestinationHealthChecker downstreamHealthChecker,
        IHealthCheckHandlePolicy healthCheckHandlePolicy)
    {
        _downstreamHealthChecker = downstreamHealthChecker;
        _healthCheckHandlePolicy = healthCheckHandlePolicy;

        _timer = new PeriodicTimer(TimeSpan.FromSeconds(2)); // 2s轮询间隔
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (await _timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
        {
            try
            {
                var state = await _downstreamHealthChecker.CheckAsync(cancellationToken).ConfigureAwait(false);
                await _healthCheckHandlePolicy.HandleAsync(state, cancellationToken).ConfigureAwait(false);
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
