using ThingsEdge.Common;
using ThingsEdge.Router.Configuration;
using ThingsEdge.Router.Devices;
using ThingsEdge.Router.Forwarder;
using ThingsEdge.Router.Handlers.Health;
using ThingsEdge.Router.Transport.MQTT;

namespace ThingsEdge.Router;

/// <summary>
/// Extensions for <see cref="IRouterBuilder"/>
/// </summary>
public static class IRouterBuilderExtensions
{
    /// <summary>
    /// 添加要注入到 EventBus 中的程序集。相同的程序集会进行去重处理。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="assembly">要添加的程序集</param>
    /// <returns></returns>
    public static IRouterBuilder AddEventBusRegisterAssembly(this IRouterBuilder builder, Assembly assembly)
    {
        if (!builder.Assemblies.Any(x => x.FullName == assembly.FullName))
        {
            builder.Assemblies.Add(assembly);
        }

        return builder;
    }

    /// <summary>
    /// 添加下游系统健康检测功能。
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRouterBuilder AddDownstreamHealthChecks(this IRouterBuilder builder)
    {
        builder.Builder.ConfigureServices(services =>
        {
            services.AddSingleton<IDestinationHealthChecker, HttpDestinationHealthChecker>();
            services.AddSingleton<IHealthCheckHandlePolicy, HealthCheckHandlePolicy>();
            services.AddHostedService<DestinationHealthCheckHostedService>();
        });
        return builder;
    }

    /// <summary>
    /// 添加设备文件提供者。
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRouterBuilder AddDeviceFileProvider(this IRouterBuilder builder)
    {
        builder.Builder.ConfigureServices(services =>
        {
            services.AddSingleton<IDeviceManager, DefaultDeviceManager>();
            services.AddSingleton<IDeviceSource, FileDeviceSource>();
        });
        return builder;
    }

    /// <summary>
    /// 添加 HTTP 服务。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="postDelegate"></param>
    /// <returns></returns>
    public static IRouterBuilder AddHttpForwarder(this IRouterBuilder builder, Action<RESTfulDestinationOptions>? postDelegate = null)
    {
        builder.Builder.ConfigureServices((hostBuilder, services) =>
        {
            services.Configure<RESTfulDestinationOptions>(hostBuilder.Configuration.GetSection("Destination"));

            services.AddSingleton<IHttpForwarder, DefalutHttpForwarder>();
            services.AddHttpClient(ForwarderConstants.HttpClientName, (sp, httpClient) =>
            {
                var options = sp.GetRequiredService<IOptions<RESTfulDestinationOptions>>().Value;
                postDelegate?.Invoke(options);
                
                httpClient.BaseAddress = new Uri(options.BaseAddress);
                if (options.EnableBasicAuth)
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Basic",
                            $"{options.UserName}:{options.Password}");
                }
            }).ConfigureHttpMessageHandlerBuilder(builder =>
            {
                var options = builder.Services.GetRequiredService<IOptions<RESTfulDestinationOptions>>().Value;
                // SocketsHttpHandler
                if (builder.PrimaryHandler is HttpClientHandler httpHandler)
                {
                    // 不验证 TLS 凭证
                    if (options.DisableCertificateValidationCheck)
                    {
                        httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                    }
                }
            });
        });

        return builder;
    }

    /// <summary>
    /// 添加 MQTT 服务。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configName">MQTT配置名称</param>
    /// <returns></returns>
    public static IRouterBuilder AddMqttForwarder(this IRouterBuilder builder, string configName = "MQTT")
    {
        builder.Builder.ConfigureServices((hostBuilder, services) =>
        {
            services.Configure<MQTTClientOptions>(hostBuilder.Configuration.GetSection(configName));
            services.AddHostedService<MQTTHostedService>();
        });

        return builder;
    }

    /// <summary>
    /// 注册基于 MediatR 的事件总线。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="assemblies">注册事件属于的程序集集合。</param>
    /// <returns></returns>
    public static IRouterBuilder AddEventBus(this IRouterBuilder builder, params Assembly[] assemblies)
    {
        builder.Builder.ConfigureServices((hostBuilder, services) =>
        {
            services.AddEventBusPublisher(); // 注入 EventBus 事件发布器。

            Assembly[] assemblies2 = assemblies?.Length > 0
                ? builder.Assemblies.Concat(assemblies).ToArray()
                : builder.Assemblies.ToArray();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies2));
        });

        return builder;
    }
}
