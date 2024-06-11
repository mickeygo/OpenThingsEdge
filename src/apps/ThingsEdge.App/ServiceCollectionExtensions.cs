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
    public static void AddScada(this IHostBuilder hostBuilder, params Assembly[] assemblies)
    {
        // 注册当前程序集
        List<Assembly> assemblyList =
        [
            typeof(ServiceCollectionExtensions).Assembly,
        ];
        if (assemblies?.Length > 0)
        {
            assemblyList.AddRange(assemblies);
        }

        hostBuilder.AddThingsEdgeRouter()
            .AddDeviceFileProvider()
            .AddNativeForwarder<ScadaNativeForwader>()
            .AddDeviceHeartbeatHandler<HeartbeatApiHandler>()
            .AddDirectMessageRequestHandler<MessageApiHandler>()
            .AddOpsProvider(option =>
            {
                option.Siemens_PDUSizeS1500 = 396; // 经测试，此 S1500 PDU 长度设置为 396 不会触发异常
            })
            .AddEventBus([.. assemblyList]);

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
