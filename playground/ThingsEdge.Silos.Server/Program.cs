using Microsoft.AspNetCore.Server.Kestrel.Core;
using ThingsEdge.Silos.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// 注册 gRPC 服务
builder.Services.AddGrpc();

builder.WebHost.UseKestrel(options =>
{
    // 5002 使用 HTTP1，用于 RESTful 
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });

    // 5002 使用 HTTP2，用于 gRPC 
    options.ListenAnyIP(5002, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2; // 明文 HTTP/2，仅内网
    });
});

var app = builder.Build();

// 注册 gRPC 终结点
app.MapGrpcService<GreeterService>();

app.MapGet("/", () =>
{
    return new
    {
        name = "ThingsEdge.Silos.Server",
        now = DateTime.Now,
    };
});

app.Run();
