using ThingsEdge.Providers.Ops.Events;

namespace ThingsEdge.Providers.Ops.DependencyInjection;

/// <summary>
/// OPS 启动时需运行的后台服务。
/// </summary>
internal sealed class OpsStartupHostedService : IHostedService
{
    private readonly EventLoop _eventLoop;

    public OpsStartupHostedService(EventLoop eventLoop)
    {
        _eventLoop = eventLoop;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            await _eventLoop.Loop(cancellationToken).ConfigureAwait(false);
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
