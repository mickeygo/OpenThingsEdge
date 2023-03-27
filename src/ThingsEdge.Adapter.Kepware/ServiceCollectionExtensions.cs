using Ops.Contrib.Kepware.IotGateway.MQTT;
using Ops.Contrib.Kepware.IotGateway.RESTful;
using ThingsEdge.Adapter.Kepware.IotGateway.RESTful;

namespace Ops.Contrib.Kepware;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 IotGateway RESTful Client 服务。
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddIotGatewayRESTfulClient(this IServiceCollection services)
    {
        services.AddHttpClient(RESTServer.HttpClientName, (sp, httpClient) =>
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
                    httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }
            }
        });

        return services;
    }

    /// <summary>
    /// 添加 IotGateway RESTful Server 服务。
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddIotGatewayRESTfulServer(this IServiceCollection services)
    {
        services.AddHttpClient(RESTServer.HttpClientName, (sp, httpClient) =>
        {
            var options = sp.GetRequiredService<IOptions<RESTfulServerOptions>>().Value;
            httpClient.BaseAddress = new Uri($"{options.RESTServerBaseAddress}/iotgateway");
        })
        .ConfigureHttpMessageHandlerBuilder(builder =>
        {
            var options = builder.Services.GetRequiredService<IOptions<RESTfulServerOptions>>().Value;

            // SocketsHttpHandler
            if (builder.PrimaryHandler is HttpClientHandler httpHandler)
            {
                if (options.DisableCertificateValidationCheck)
                {
                    httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }

                if (!options.AllowAnonymous)
                {
                    var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate(options.CertificatePath);
                    httpHandler.ClientCertificates.Add(certificate);
                }
            }
        });

        services.AddTransient<IRESTServerApi, RESTServer>();

        return services;
    }

    public static IServiceCollection AddIotGatewayMQTT(this IServiceCollection services)
    {
        MQTTClientOptions options = new();
        MQTTClientFactory.Create(options);

        return services;
    }
}
