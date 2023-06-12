using ThingsEdge.Application.Configuration;
using ThingsEdge.Application.Handlers;
using ThingsEdge.Router.Interfaces;

namespace ThingsEdge.Application;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册 ThingsEdge 应用服务。
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="configName">配置名称，默认为"App"</param>
    /// <returns></returns>
    public static void AddThingsEdgeApplication(this IHostBuilder hostBuilder, string configName = "App")
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            services.Configure<ApplicationConfig>(context.Configuration.GetSection(configName));

            services.AddSqlSugarSetup();
            services.AddAutoDependencyInjection(typeof(ServiceCollectionExtensions).Assembly);

            // 注册处理 Api
            services.AddTransient<IDeviceHeartbeatApi, DeviceHeartbeatApiHandler>();
            services.AddTransient<IMessageRequestApi, MessageRequestApiHandler>();
        });
    }
}
