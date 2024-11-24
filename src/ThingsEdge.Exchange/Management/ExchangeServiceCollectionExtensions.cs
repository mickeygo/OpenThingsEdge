using ThingsEdge.Exchange.Configuration;
using ThingsEdge.Exchange.Engine;
using ThingsEdge.Exchange.Engine.Monitors;
using ThingsEdge.Exchange.Engine.Snapshot;
using ThingsEdge.Exchange.Events;
using ThingsEdge.Exchange.Forwarders;
using ThingsEdge.Exchange.Infrastructure.EventBus;
using ThingsEdge.Exchange.Interfaces;
using ThingsEdge.Exchange.Interfaces.Impls;
using ThingsEdge.Exchange.Internal;
using ThingsEdge.Exchange.Management;

namespace ThingsEdge.Exchange;

/// <summary>
/// Extensions for <see cref="IServiceCollection"/>
/// </summary>
public static class ExchangeServiceCollectionExtensions
{
    /// <summary>
    /// 添加 ThingsEdge 组件。
    /// </summary>
    /// <param name="builder">HostBuilder</param>
    /// <param name="builderAction">配置</param>
    /// <returns></returns>
    public static IHostBuilder AddThingsEdgeExchange(this IHostBuilder builder, Action<IExchangeBuilder> builderAction)
    {
        // 注册监控器
        MonitorLoop.Register<HeartbeatMonitor>();
        MonitorLoop.Register<NoticeMonitor>();
        MonitorLoop.Register<TriggerMonitor>();
        MonitorLoop.Register<SwitchMonitor>();

        // 注册服务
        builder.ConfigureServices((hostBuilder, services) =>
        {
            // 缓存
            services.AddMemoryCache();

            // 注册基于 MediatR 的事件总线。
            services.AddSingleton<IEventPublisher, EventPublisher>();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ExchangeServiceCollectionExtensions).Assembly));

            // 注册内置服务。
            services.AddTransient<HeartbeatMonitor>();
            services.AddTransient<MonitorLoop>();
            services.AddTransient<NoticeMonitor>();
            services.AddTransient<TriggerMonitor>();
            services.AddTransient<SwitchMonitor>();

            services.AddSingleton<DriverConnectorManager>();
            services.AddSingleton<EngineExchange>();
            services.AddSingleton<EventDispatcher>();
            services.AddSingleton<EventLoop>();
            services.AddSingleton<InternalEventBroker>();
            services.AddSingleton<IProducer, InternalProducer>();
            services.AddSingleton<INotificationForwarderWrapper, NotificationForwarderWrapper>();
            services.AddSingleton<IRequestForwarderProvider,  RequestForwarderProvider>();
            services.AddSingleton<ITagReaderWriter, TagReaderWriterImpl>();
            services.AddSingleton<ITagDataSnapshot, TagDataSnapshotImpl>();
        });

        var builder2 = new ExchangeBuilder(builder);
        builderAction(builder2);

        return builder;
    }

    /// <summary>
    /// 设置参数。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="optionsAction">参数选项设置</param>
    /// <param name="configName">配置节点名称，默认为 "Exchange"。</param>
    /// <returns></returns>
    public static IExchangeBuilder UseOptions(this IExchangeBuilder builder, Action<ExchangeOptions>? optionsAction = null, string configName = "Exchange")
    {
        builder.Builder.ConfigureServices((hostBuilder, services) =>
        {
            services.Configure<ExchangeConfig>(hostBuilder.Configuration.GetSection(configName));

            if (optionsAction != null)
            {
                services.PostConfigure(optionsAction);
            }

            services.AddHostedService<StartupHostedService>();
        });

        // 选项配置
        ExchangeOptions options = new();
        optionsAction?.Invoke(options);

        GlobalSettings.HeartbeatShouldAckZero = options.HeartbeatShouldAckZero;

        if (options.TagTriggerConditionValue > 0)
        {
            GlobalSettings.TagTriggerConditionValue = options.TagTriggerConditionValue;
        }
        if (options.Siemens_PDUSizeS1500 > 0)
        {
            GlobalSettings.SiemensS7NetOptions.S1500_PDUSize = options.Siemens_PDUSizeS1500;
        }
        if (options.Siemens_PDUSizeS1200 > 0)
        {
            GlobalSettings.SiemensS7NetOptions.S1200_PDUSize = options.Siemens_PDUSizeS1200;
        }
        if (options.Siemens_PDUSizeS300 > 0)
        {
            GlobalSettings.SiemensS7NetOptions.S300_PDUSize = options.Siemens_PDUSizeS300;
        }

        return builder;
    }
}
