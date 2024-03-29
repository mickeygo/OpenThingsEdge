﻿using ThingsEdge.Providers.Kepware.IotGateway.RESTful;

namespace ThingsEdge.Providers.Kepware;

public static class KepwareServiceCollectionExtensions
{
    /// <summary>
    /// 添加 Kepware IotGateway RESTful Server 服务。
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddIotGatewayRESTfulServer(this IServiceCollection services)
    {
        services.AddHttpClient(DefaultRESTServer.HttpClientName, (sp, httpClient) =>
        {
            var options = sp.GetRequiredService<IOptions<RESTfulServerOptions>>().Value;
            httpClient.BaseAddress = new Uri($"{options.RESTServerBaseAddress}/iotgateway");
        })
        .ConfigurePrimaryHttpMessageHandler((handler, sp) =>
        {
            var options = sp.GetRequiredService<IOptions<RESTfulServerOptions>>().Value;

            // SocketsHttpHandler
            if (handler is HttpClientHandler httpHandler)
            {
                if (options.DisableCertificateValidationCheck)
                {
                    httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }

                if (!options.AllowAnonymous && !string.IsNullOrEmpty(options.CertFileName))
                {
                    var certificate = new X509Certificate(options.CertFileName, options.CertPassword);
                    httpHandler.ClientCertificates.Add(certificate);
                }
            }
        });

        services.AddTransient<IRESTServerApi, DefaultRESTServer>();

        return services;
    }
}
