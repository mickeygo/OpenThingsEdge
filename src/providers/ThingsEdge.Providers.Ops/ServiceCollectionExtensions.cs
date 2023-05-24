using ThingsEdge.Providers.Ops.Configuration;
using ThingsEdge.Providers.Ops.Exchange;
using ThingsEdge.Providers.Ops.Handlers;
using ThingsEdge.Router.Devices;

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

            services.AddSingleton<IExchange, OpsExchange>();
            services.AddSingleton<DriverConnectorManager>();
            services.AddSingleton<SwitchContainer>();
            services.AddSingleton<CurveStorage>();
            services.AddSingleton<IDeviceReadWrite, OpsDeviceReadWrite>();
        });

        return builder;
    }
}
