using ThingsEdge.ConsoleApp.Addresses;
using ThingsEdge.ConsoleApp.Configuration;
using ThingsEdge.ConsoleApp.Forwarders;
using ThingsEdge.ConsoleApp.Handlers;
using ThingsEdge.ConsoleApp.HostedServices;
using ThingsEdge.Exchange;

namespace ThingsEdge.ConsoleApp;

/// <summary>
/// 
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册 Scada 服务。
    /// </summary>
    /// <param name="hostBuilder"></param>
    public static void AddScada(this IHostBuilder hostBuilder)
    {
        hostBuilder.AddThingsEdgeExchange(builder =>
        {
            builder.UseDeviceCustomProvider<ModbusTcpAddressProvider>()
                .UseDeviceHeartbeatForwarder<HeartbeatForwarder>()
                .UseNativeNoticeForwarder<NoticeForwarder>()
                .UseNativeTriggerForwarder<TriggerForwader>();
        });

        hostBuilder.ConfigureServices((context, services) =>
        {
            services.Configure<ScadaConfig>(context.Configuration.GetSection("Scada"));

            services.AddTransient<ArchiveHandler>();

            // 启动项
            services.AddHostedService<AppStartupHostedService>();
        });
    }
}
