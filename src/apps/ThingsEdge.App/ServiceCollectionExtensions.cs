using ThingsEdge.App.Configuration;
using ThingsEdge.App.Forwarders;
using ThingsEdge.App.Handlers;
using ThingsEdge.App.HostedServices;
using ThingsEdge.Common.DependencyInjection;
using ThingsEdge.Router;

namespace ThingsEdge.App;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册 Scada 服务。
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="assemblies">注册 EventBus 的程序集</param>
    public static void AddScada(this IHostBuilder hostBuilder)
    {
        hostBuilder.AddThingsEdgeRouter()
            .AddDeviceFileProvider()
            .AddDeviceHeartbeatHandler<HeartbeatApiHandler>()
            .AddNativeTriggerForwarder<ScadaNativeForwader>()
            .AddNativeNoticeForwarder<NoticeApiHandler>()
            .AddOpsProvider(option =>
            {
                option.Siemens_PDUSizeS1500 = 396; // S7 PDU 长度
            })
            .AddEventBus();

        hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddScadaServices(context.Configuration);
        });
    }

    private static IServiceCollection AddScadaServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ScadaConfig>(configuration.GetSection("Scada"));

        // 注册标记的服务
        services.AddAutoDependencyInjection(typeof(ServiceCollectionExtensions).Assembly);

        // 启动项
        services.AddHostedService<AppStartupHostedService>();

        return services;
    }
}
