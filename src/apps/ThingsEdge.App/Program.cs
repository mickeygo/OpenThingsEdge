using ThingsEdge.App;

var host = Host.CreateDefaultBuilder();

host.ConfigureServices(services => { 
    services.AddMemoryCache();
});

// 注册本地Scada服务
host.AddScada(Assembly.GetExecutingAssembly());

await host.RunConsoleAsync();
