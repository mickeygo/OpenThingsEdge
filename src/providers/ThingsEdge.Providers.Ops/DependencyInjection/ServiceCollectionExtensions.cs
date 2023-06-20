using ThingsEdge.Providers.Ops.Configuration;
using ThingsEdge.Providers.Ops.Events;
using ThingsEdge.Providers.Ops.Exchange.Monitors;

namespace ThingsEdge.Router;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 OPS Provider 服务。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configName">配置节点名称。</param>
    /// <returns></returns>
    public static IRouterBuilder AddOpsProvider(this IRouterBuilder builder, string configName = "Ops")
    {
        builder.AddEventBusRegisterAssembly(typeof(ServiceCollectionExtensions).Assembly);

        builder.Builder.ConfigureServices((hostBuilder, services) =>
        {
            services.Configure<OpsConfig>(hostBuilder.Configuration.GetSection(configName));
            services.AddAutoDependencyInjection(typeof(ServiceCollectionExtensions).Assembly);
            services.AddHostedService<OpsHostedService>();
        });

        // 注册监控器
        MonitorLoop.Register<HeartbeatMonitor>();
        MonitorLoop.Register<NoticeMonitor>();
        MonitorLoop.Register<TriggerMonitor>();
        MonitorLoop.Register<SwitchMonitor>();

        return builder;
    }
}

internal sealed class OpsHostedService : IHostedService
{
    private readonly EventLoop _eventLoop;

    public OpsHostedService(EventLoop eventLoop)
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
