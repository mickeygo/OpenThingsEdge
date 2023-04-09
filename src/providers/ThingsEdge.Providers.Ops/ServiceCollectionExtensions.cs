using ThingsEdge.Providers.Ops.Exchange;

namespace ThingsEdge.Providers.Ops;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 OPS Provider 服务。
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddOps(this IServiceCollection services)
    {
        services.AddOptions<OpsConfig>();

        services.AddSingleton<IEngine, Engine>();
        services.AddSingleton<DriverConnectorManager>();
        services.AddSingleton<IMessagePusher, MessagePusher>();

        return services;
    }
}
