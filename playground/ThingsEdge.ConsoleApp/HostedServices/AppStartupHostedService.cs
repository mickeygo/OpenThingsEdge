using ThingsEdge.ConsoleApp.Configuration;
using ThingsEdge.Exchange.Interfaces;

namespace ThingsEdge.ConsoleApp.HostedServices;

/// <summary>
/// 程序启动后台服务
/// </summary>
internal sealed class AppStartupHostedService(IExchange exchange,
    IOptions<ScadaConfig> options,
    ILogger<AppStartupHostedService> logger) : IHostedService
{
    private readonly ScadaConfig _config = options.Value;
    private readonly ILogger _logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_config.IsAutoStartup)
        {
            return;
        }

        try
        {
            await Task.Delay(3000, cancellationToken).ConfigureAwait(false); // 延迟启动

            if (!exchange.IsRunning)
            {
                _logger.LogInformation("SCADA 自动服务启动中。。。");

                await exchange.StartAsync().ConfigureAwait(false);

                _logger.LogInformation("SCADA 服务已启动");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AppStartupHostedService] SCADA 服务启动失败。");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_config.IsAutoStartup && exchange.IsRunning)
        {
            await exchange.ShutdownAsync().ConfigureAwait(false);

            _logger.LogInformation("SCADA 服务已关闭");
        }
    }
}
