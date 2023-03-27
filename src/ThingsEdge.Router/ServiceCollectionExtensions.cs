using ThingsEdge.Router.Transport.RESTful;

namespace ThingsEdge.Router;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 RESTful Client 服务。
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddRESTfulClient(this IServiceCollection services)
    {
        services.AddHttpClient("ThingsEdge.Router.RESTfulClient", (sp, httpClient) =>
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
    public static IServiceCollection AddMQTTClient(this IServiceCollection services)
    {
        return services;
    }
}
