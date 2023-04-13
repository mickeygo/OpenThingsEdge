using ThingsEdge.Router.Configuration;
using ThingsEdge.Router.Devices;
using ThingsEdge.Router.Forwarder;
using ThingsEdge.Router.Transport.MQTT;
using ThingsEdge.Router.Transport.RESTful;

namespace ThingsEdge.Router;

/// <summary>
/// Extensions for <see cref="IServiceCollection"/>
/// </summary>
public static class RouterServiceCollectionExtensions
{
    /// <summary>
    /// 添加 ThingsEdge 路由。
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddThingsEdgeRouter(this IServiceCollection services)
    {
        services.AddSingleton<IDeviceSource, FileDeviceSource>();
        services.AddSingleton<IDeviceManager, DefaultDeviceManager>();
        services.AddSingleton<IHttpForwarder, DefalutHttpForwarder>();

        services.AddRESTfulClient();

        return services;
    }

    /// <summary>
    /// 添加 RESTful Client 服务。
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddRESTfulClient(this IServiceCollection services)
    {
        services.AddHttpClient(ForwarderConstants.HttpClientName, (sp, httpClient) =>
        {
            var options = sp.GetRequiredService<IOptions<RESTfulClientOptions>>().Value;
            httpClient.BaseAddress = new Uri($"{options.RESTClientBaseAddress}");
            if (!string.IsNullOrEmpty(options.UserName) && !string.IsNullOrEmpty(options.Password))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                        "Basic",
                        $"{options.UserName}:{options.Password}");
            }
        }).ConfigureHttpMessageHandlerBuilder(builder =>
        {
            var options = builder.Services.GetRequiredService<IOptions<RESTfulClientOptions>>().Value;
            // SocketsHttpHandler
            if (builder.PrimaryHandler is HttpClientHandler httpHandler)
            {
                if (options.DisableCertificateValidationCheck)
                {
                    // 不验证 TLS 凭证
                    httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }
            }
        });

        return services;
    }

    /// <summary>
    /// 添加 MQTT 服务。
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddMQTTClient(this IServiceCollection services, IConfiguration configuration, string mqtt = "MQTT")
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(typeof(RouterServiceCollectionExtensions).Assembly);
        });

        services.Configure<MQTTClientOptions>(configuration.GetSection(mqtt));
        services.AddHostedService<MQTTHostedService>();

        return services;
    }
}
