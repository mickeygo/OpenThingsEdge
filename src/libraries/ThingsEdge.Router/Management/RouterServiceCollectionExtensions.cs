using ThingsEdge.Router.Management;

namespace ThingsEdge.Router;

/// <summary>
/// Extensions for <see cref="IServiceCollection"/>
/// </summary>
public static class RouterServiceCollectionExtensions
{
    /// <summary>
    /// 添加 ThingsEdge 路由。
    /// </summary>
    /// <param name="builder">HostBuilder</param>
    /// <returns></returns>
    public static IRouterBuilder AddThingsEdgeRouter(this IHostBuilder builder)
    {
        var builder2 = new RouterBuilder(builder);

        // 注册 EventBus 程序集。
        builder2.AddEventBusRegisterAssembly(typeof(RouterServiceCollectionExtensions).Assembly); // 注册 ThingsEdge.Router 程序集

        // 注册全局服务
        builder2.Builder.ConfigureServices((_, services) =>
        {
            services.AddAutoDependencyInjection(typeof(RouterServiceCollectionExtensions).Assembly);
        });

        return builder2;
    }
}
