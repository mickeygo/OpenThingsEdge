using Serilog;
using ThingsEdge.ConsoleApp.Addresses;
using ThingsEdge.ConsoleApp.Configuration;
using ThingsEdge.ConsoleApp.Forwarders;
using ThingsEdge.ConsoleApp.Handlers;
using ThingsEdge.ConsoleApp.HostedServices;
using ThingsEdge.Exchange;

var host = Host.CreateDefaultBuilder();

host.ConfigureServices(services => {
    services.AddMemoryCache();
})
.UseSerilog((hostingContext, loggerConfiguration) => 
    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration)
);

// 注册本地Scada服务
host.ConfigureServices((context, services) =>
{
    services.Configure<ScadaConfig>(context.Configuration.GetSection("Scada"));

    services.AddTransient<ArchiveHandler>();

    // 启动项
    services.AddHostedService<AppStartupHostedService>();
});

host.AddThingsEdgeExchange(static builder =>
{
    builder.UseDeviceCustomProvider<ModbusTcpAddressProvider>()
        .UseDeviceHeartbeatForwarder<HeartbeatForwarder>()
        .UseNativeNoticeForwarder<NoticeForwarder>()
        .UseNativeTriggerForwarder<TriggerForwader>()
        .UseNativeSwitchForwarder<SwitchForwarder>()
        .UseOptions(options =>
        {
            options.SocketPoolSize = 5;
            options.Curve.FileType = ThingsEdge.Exchange.Configuration.CurveFileExt.JSON;
        });
});

await host.RunConsoleAsync().ConfigureAwait(false);
