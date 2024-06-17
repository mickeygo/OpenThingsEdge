using ThingsEdge.Common;
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
    /// <param name="builderAction">配置</param>
    /// <returns></returns>
    public static IHostBuilder AddThingsEdgeRouter(this IHostBuilder builder, Action<IRouterBuilder> builderAction)
    {
        var builder2 = new RouterBuilder(builder);

        // 注册 EventBus 程序集。
        builder2.AddEventBusRegisterAssembly(typeof(RouterServiceCollectionExtensions).Assembly); // 注册 ThingsEdge.Router 程序集

        // 注册全局服务
        builder2.Builder.ConfigureServices((_, services) =>
        {
            services.AddAutoDependencyInjection(typeof(RouterServiceCollectionExtensions).Assembly);
        });

        builderAction(builder2);
        builder2.AddEventBus(); // 需要在末尾注册

        return builder;
    }

    /// <summary>
    /// 注册基于 MediatR 的事件总线。
    /// </summary>
    private static IRouterBuilder AddEventBus(this IRouterBuilder builder)
    {
        builder.Builder.ConfigureServices((hostBuilder, services) =>
        {
            services.AddEventBusPublisher(); // 注入 EventBus 事件发布器。
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies([.. builder.EventAssemblies]));
        });

        return builder;
    }
}
