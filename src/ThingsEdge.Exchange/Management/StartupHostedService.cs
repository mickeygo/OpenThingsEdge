using ThingsEdge.Exchange.Engine;

namespace ThingsEdge.Exchange.Management;

/// <summary>
/// OPS 启动时需运行的后台服务。
/// </summary>
internal sealed class StartupHostedService(IMessageLoop messageLoop) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            await messageLoop.LoopAsync(cancellationToken).ConfigureAwait(false);
        }, cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
