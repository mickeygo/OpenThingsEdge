using ThingsEdge.Contrib.Http.Configuration;
using ThingsEdge.Contrib.Http.Forwarders;
using ThingsEdge.Contrib.Http.Health;
using ThingsEdge.Router.Forwarders;

namespace ThingsEdge.Router;

public static class IHttpRouterBuilderExtensions
{
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
    /// 添加 HTTP 数据发送服务，其中 <see cref="TagFlag.Trigger"/> 会发布此事件。
    /// </summary>
    /// <typeparam name="TForwarder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="postDelegate">配置后更改委托</param>
    /// <param name="lifetime"></param>
    /// <param name="configName">配置名称</param>
    /// <returns></returns>
    public static IRouterBuilder AddHttpTriggerForwarder<TForwarder>(this IRouterBuilder builder,
        Action<RESTfulDestinationOptions>? postDelegate = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient,
        string configName = "HttpDestination")
        where TForwarder : IRequestForwarder
    {
        builder.Builder.ConfigureServices((hostBuilder, services) =>
        {
            services.Configure<RESTfulDestinationOptions>(hostBuilder.Configuration.GetSection(configName));
            if (postDelegate is not null)
            {
                services.PostConfigure(postDelegate);
            }

            services.Add(ServiceDescriptor.DescribeKeyed(typeof(IRequestForwarder), "HTTP", typeof(TForwarder), lifetime));
            ForwarderRegisterHub.Default.Register("HTTP");

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
            })
            .ConfigurePrimaryHttpMessageHandler((handler, sp) =>
            {
                if (handler is HttpClientHandler httpHandler)
                {
                    var options = sp.GetRequiredService<IOptions<RESTfulDestinationOptions>>().Value;
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
    /// 添加 HTTP 数据发送服务，其中 <see cref="TagFlag.Trigger"/> 会发布此事件。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="postDelegate">配置后更改委托</param>
    /// <param name="lifetime"></param>
    /// <param name="configName">配置名称</param>
    /// <returns></returns>
    public static IRouterBuilder AddHttpTriggerForwarder(this IRouterBuilder builder,
        Action<RESTfulDestinationOptions>? postDelegate = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient,
        string configName = "HttpDestination")
    {
        return builder.AddHttpTriggerForwarder<HttpRequestForwarder>(postDelegate, lifetime, configName);
    }
}
