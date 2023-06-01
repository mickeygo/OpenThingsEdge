using ThingsEdge.Providers.Ops.Configuration;
using ThingsEdge.Providers.Ops.Exchange;

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

            services.AddSingleton<DriverConnectorManager>();
        });

        return builder;
    }
}
