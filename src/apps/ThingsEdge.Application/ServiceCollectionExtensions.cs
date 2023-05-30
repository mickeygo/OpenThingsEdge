using ThingsEdge.Application.Configuration;
using ThingsEdge.Application.Handlers;
using ThingsEdge.Application.Management.Equipment;
using ThingsEdge.Router.Interfaces;

namespace ThingsEdge.Application;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册 ThingsEdge 应用服务。
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="configName">配置名称</param>
    /// <returns></returns>
    public static void AddThingsEdgeApplication(this IHostBuilder hostBuilder, string configName = "AppConfig")
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            services.Configure<ApplicationConfig>(context.Configuration.GetSection(configName));

            services.AddSqlSugarSetup();

            services.AddThingsEdgeDomainService();
            services.AddThingsEdgeManagement();

            services.AddTransient<EquipmentStateManager>();

            // 注册处理 Api
            services.AddTransient<IDeviceHeartbeatApi, DeviceHeartbeatApiHandler>();
            services.AddTransient<IMessageRequestPostingApi, MessageRequestPostingApiHandler>();
        });
    }

    /// <summary>
    /// 注册领域服务对象。
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    private static IServiceCollection AddThingsEdgeDomainService(this IServiceCollection services)
    {
        // 添加自定义服务
        var types = typeof(IDomainService).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface && typeof(IDomainService).IsAssignableFrom(t));
        foreach (var type in types)
        {
            var interfaceType = type.GetInterfaces().FirstOrDefault(t => typeof(IDomainService).IsAssignableFrom(t) && t != typeof(IDomainService));
            if (interfaceType != null)
            {
                services.AddTransient(interfaceType, type);
            }
        }

        return services;
    }

    /// <summary>
    /// 注册管理对象。
    /// </summary>s
    private static IServiceCollection AddThingsEdgeManagement(this IServiceCollection services)
    {
        var types = typeof(IManager).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface && typeof(IManager).IsAssignableFrom(t));
        foreach (var type in types)
        {
            services.AddSingleton(type); // 单例模式
        }

        return services;
    }
}
