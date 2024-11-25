using Serilog;
using ThingsEdge.ConsoleApp;

var host = Host.CreateDefaultBuilder();

host.ConfigureServices(services => {
    services.AddMemoryCache();
})
.UseSerilog((hostingContext, loggerConfiguration) => 
    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration)
);

// 注册本地Scada服务
host.AddScada();

await host.RunConsoleAsync();
