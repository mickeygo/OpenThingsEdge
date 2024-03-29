﻿using ThingsEdge.Common;
using ThingsEdge.Router.Configuration;
using ThingsEdge.Router.Devices;
using ThingsEdge.Router.Forwarder;
using ThingsEdge.Router.Handlers.Health;
using ThingsEdge.Router.Interfaces;
using ThingsEdge.Router.Transport.MQTT;

namespace ThingsEdge.Router;

/// <summary>
/// Extensions for <see cref="IRouterBuilder"/>
/// </summary>
public static class IRouterBuilderExtensions
{
    /// <summary>
    /// 添加要注入到 EventBus 中的程序集。可以注册多个，相同的程序集会进行去重处理。
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
    /// 添加设备基于本地文件的提供者。
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRouterBuilder AddDeviceFileProvider(this IRouterBuilder builder)
    {
        builder.Builder.ConfigureServices(services =>
        {
            services.AddSingleton<IDeviceFactory, DefaultDeviceFactory>();
            services.AddSingleton<IDeviceProvider, FileDeviceProvider>();
        });
        return builder;
    }

    /// <summary>
    /// 添加自定义设备提供者。
    /// </summary>
    /// <typeparam name="TProvider"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRouterBuilder AddDeviceCustomProvider<TProvider>(this IRouterBuilder builder)
        where TProvider : class, IDeviceProvider
    {
        builder.Builder.ConfigureServices(services =>
        {
            services.AddSingleton<IDeviceFactory, DefaultDeviceFactory>();
            services.AddSingleton<IDeviceProvider, TProvider>();
        });
        return builder;
    }

    /// <summary>
    /// 添加 HTTP 转发服务，其中 <see cref="TagFlag.Notice"/> 和 <see cref="TagFlag.Trigger"/> 会发布此事件。
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

                // 不验证 TLS 凭证
                if (options.DisableCertificateValidationCheck)
                {

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
    /// 添加 MQTT 客户端转发服务，其中 <see cref="TagFlag.Notice"/> 和 <see cref="TagFlag.Trigger"/> 会发布此事件。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="postDelegate">配置后更改委托</param>
    /// <param name="configName">MQTT Broker 配置名称</param>
    /// <returns></returns>
    public static IRouterBuilder AddMqttClientForwarder(this IRouterBuilder builder, 
        Action<MQTTClientOptions>? postDelegate = null, string configName = "MqttBroker")
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
    /// 添加本地的转发服务，其中 <see cref="TagFlag.Notice"/> 和 <see cref="TagFlag.Trigger"/> 会发布此事件。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="lifetime"><typeparamref name="TForwarder"/>注册的生命周期。</param>
    /// <returns></returns>
    public static IRouterBuilder AddNativeForwarder<TForwarder>(this IRouterBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TForwarder : INativeForwarder
    {
        builder.Builder.ConfigureServices(services =>
        {
            services.Add(new ServiceDescriptor(typeof(INativeForwarder), typeof(TForwarder), lifetime));
            InternalForwarderHub.Default.Register<NativeForwarder>(); // 注册 Native Forwarder
        });

        return builder;
    }

    /// <summary>
    /// 添加设备心跳信息处理服务，其中 <see cref="TagFlag.Heartbeat"/> 会发布此事件。
    /// </summary>
    /// <typeparam name="IHandler"></typeparam>
    /// <param name="builder"></param>
    /// <param name="lifetime"><typeparamref name="IHandler"/>注册的生命周期。</param>
    /// <returns></returns>
    public static IRouterBuilder AddDeviceHeartbeatHandler<IHandler>(this IRouterBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where IHandler : IDeviceHeartbeatApi
    {
        builder.Builder.ConfigureServices(services =>
        {
            services.Add(new ServiceDescriptor(typeof(IDeviceHeartbeatApi), typeof(IHandler), lifetime));
        });

        return builder;
    }

    /// <summary>
    /// 添加设备心跳信息处理服务，其中 <see cref="TagFlag.Notice"/>、<see cref="TagFlag.Trigger"/> 和 <see cref="TagFlag.Switch"/> 会发布此事件。
    /// </summary>
    /// <typeparam name="IHandler"></typeparam>
    /// <param name="builder"></param>
    /// <param name="lifetime"><typeparamref name="IHandler"/>注册的生命周期。</param>
    /// <returns></returns>
    public static IRouterBuilder AddDirectMessageRequestHandler<IHandler>(this IRouterBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where IHandler : IDirectMessageRequestApi
    {
        builder.Builder.ConfigureServices(services =>
        {
            services.Add(new ServiceDescriptor(typeof(IDirectMessageRequestApi), typeof(IHandler), lifetime));
        });

        return builder;
    }

    /// <summary>
    /// 添加曲线文件信息处理服务，其中 <see cref="TagFlag.Switch"/> 会发布此事件。
    /// </summary>
    /// <typeparam name="IHandler"></typeparam>
    /// <param name="builder"></param>
    /// <param name="lifetime"><typeparamref name="IHandler"/>注册的生命周期。</param>
    /// <returns></returns>
    public static IRouterBuilder AddCurveFilePostedHandler<IHandler>(this IRouterBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Transient)
       where IHandler : ICurveFilePostedApi
    {
        builder.Builder.ConfigureServices(services =>
        {
            services.Add(new ServiceDescriptor(typeof(ICurveFilePostedApi), typeof(IHandler), lifetime));
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
                ? [.. builder.EventAssemblies, .. assemblies]
                : builder.EventAssemblies.ToArray();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies2));
        });

        return builder;
    }
}
