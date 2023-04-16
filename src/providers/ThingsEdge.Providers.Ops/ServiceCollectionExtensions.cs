using ThingsEdge.Providers.Ops.Exchange;
using ThingsEdge.Providers.Ops.Handlers;
using ThingsEdge.Router;

namespace ThingsEdge.Providers.Ops;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 OPS Provider 服务。
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration">配置。</param>
    /// <param name="name">配置节点名称。</param>
    /// <returns></returns>
    public static IServiceCollection AddOpsProvider(this IServiceCollection services, IConfiguration configuration, string name = "Ops")
    {
        services.Configure<OpsConfig>(configuration.GetSection(name));

        services.AddSingleton<IExchange, OpsExchange>();
        services.AddSingleton<DriverConnectorManager>();
        services.AddSingleton<SwitchContainer>();

        return services;
    }
}
