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
        if (!builder.EventAssemblies.Any(x => x.FullName == assembly.FullName))
        {
            builder.EventAssemblies.Add(assembly);
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
            services.AddSingleton<IDeviceProvider, FileDeviceProvider>();
        });
        return builder;
    }

    /// <summary>
    /// 添加 HTTP 转发服务。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="postDelegate">配置后更改委托</param>
    /// <param name="configName">配置名称</param>
    /// <returns></returns>
    public static IRouterBuilder AddHttpForwarder(this IRouterBuilder builder, 
        Action<RESTfulDestinationOptions>? postDelegate = null, string configName = "HttpDestination")
    {
        builder.Builder.ConfigureServices((hostBuilder, services) =>
        {
            services.Configure<RESTfulDestinationOptions>(hostBuilder.Configuration.GetSection(configName));
            if (postDelegate is not null)
            {
                services.PostConfigure(postDelegate);
            }

            InternalForwarderHub.Default.Register<HttpForwarder>(); // 注册 Http Forwarder

            // 配置 HttpClient
            services.AddHttpClient(ForwarderConstants.HttpClientName, (sp, httpClient) =>
            {
                var options = sp.GetRequiredService<IOptions<RESTfulDestinationOptions>>().Value;
                httpClient.BaseAddress = new Uri(options.BaseAddress);

                if (options.Timeout > 0)
                {
                    httpClient.Timeout = TimeSpan.FromMilliseconds(options.Timeout.Value);
                }

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
    /// 添加 MQTT 客户端转发服务。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="postDelegate">配置后更改委托</param>
    /// <param name="configName">MQTT配置名称</param>
    /// <returns></returns>
    public static IRouterBuilder AddMqttClientForwarder(this IRouterBuilder builder, 
        Action<MQTTClientOptions>? postDelegate = null, string configName = "MQTT")
    {
        builder.Builder.ConfigureServices((hostBuilder, services) =>
        {
            services.Configure<MQTTClientOptions>(hostBuilder.Configuration.GetSection(configName));
            if (postDelegate is not null)
            {
                services.PostConfigure(postDelegate);
            }
            
            InternalForwarderHub.Default.Register<MqttClientForwarder>(); // 注册 MQTT Forwarder

            // 注册并启动托管的 MQTT 客户端
            services.AddSingleton<IMQTTManagedClient, MQTTManagedClient>(sp =>
            {
                var mqttClientOptions = sp.GetRequiredService<IOptions<MQTTClientOptions>>().Value;
                var client = (MQTTManagedClient)MQTTClientFactory.CreateManagedClient(mqttClientOptions);
                client.StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                return client;
            });
        });

        return builder;
    }

    /// <summary>
    /// 添加自定义的转发服务。
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRouterBuilder AddCustomForwarder<TForwarder>(this IRouterBuilder builder)
        where TForwarder : class, IForwarder
    {
        builder.Builder.ConfigureServices(services =>
        {
            InternalForwarderHub.Default.Register<TForwarder>();
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
                ? builder.EventAssemblies.Concat(assemblies).ToArray()
                : builder.EventAssemblies.ToArray();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies2));
        });

        return builder;
    }
}
