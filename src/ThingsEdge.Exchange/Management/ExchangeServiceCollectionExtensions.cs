using ThingsEdge.Exchange.Configuration;
using ThingsEdge.Exchange.Engine;
using ThingsEdge.Exchange.Engine.Connectors;
using ThingsEdge.Exchange.Engine.Handlers;
using ThingsEdge.Exchange.Engine.Snapshot;
using ThingsEdge.Exchange.Engine.Workers;
using ThingsEdge.Exchange.Forwarders;
using ThingsEdge.Exchange.Infrastructure.Brokers;
using ThingsEdge.Exchange.Interfaces;
using ThingsEdge.Exchange.Interfaces.Impls;
using ThingsEdge.Exchange.Management;
using ThingsEdge.Exchange.Storages.Curve;

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
        EngineExecutor.Register<HeartbeatWorker>();
        EngineExecutor.Register<NoticeWorker>();
        EngineExecutor.Register<TriggerWorker>();
        EngineExecutor.Register<SwitchWorker>();

        // 注册服务
        builder.ConfigureServices((hostBuilder, services) =>
        {
            // 注册配置选项
            services.Configure<ExchangeOptions>(hostBuilder.Configuration.GetSection("Exchange"));

            // 注册缓存
            services.AddMemoryCache();

            // 注册后台启动项
            services.AddHostedService<StartupHostedService>();

            // 注册消息队列服务
            services.AddSingleton(typeof(IMessageBroker<>), typeof(SingleMessageBroker<>));

            // 注册内置服务。
            services.AddTransient<EngineExecutor>();
            services.AddSingleton<HeartbeatWorker>();
            services.AddSingleton<NoticeWorker>();
            services.AddSingleton<TriggerWorker>();
            services.AddSingleton<SwitchWorker>();

            services.AddTransient<IHeartbeatMessageHandler, HeartbeatMessageHandler>();
            services.AddTransient<INoticeMessageHandler, NoticeMessageHandler>();
            services.AddTransient<ITriggerMessageHandler, TriggerMessageHandler>();
            services.AddTransient<ISwitchMessageHandler, SwitchMessageHandler>();

            services.AddSingleton<IDriverConnectorManager, DriverConnectorManager>();
            services.AddSingleton<IExchange, EngineExchange>();
            services.AddTransient<ITagReaderWriter, TagReaderWriterImpl>();

            services.AddSingleton<ITagDataSnapshot, TagDataSnapshotImpl>();
            services.AddSingleton<IMessageLoop, MessageLoop>();

            services.AddTransient<IHeartbeatForwarderProxy, HeartbeatForwarderProxy>();
            services.AddTransient<INoticeForwarderProxy, NoticeForwarderProxy>();
            services.AddTransient<ITriggerForwarderProxy, TriggerForwarderProxy>();
            services.AddTransient<ISwitchForwarderProxy, SwitchForwarderProxy>();

            services.AddSingleton<CurveStorage>();
            services.AddSingleton<CurveContainer>();
        });

        var builder2 = new ExchangeBuilder(builder);
        builderAction.Invoke(builder2);

        return builder;
    }

    /// <summary>
    /// 设置参数，其中配置的参数会覆盖配置文件 "Exchange" 选项设置。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="optionsAction">参数选项设置</param>
    /// <returns></returns>
    public static IExchangeBuilder UseOptions(this IExchangeBuilder builder, Action<ExchangeOptions>? optionsAction = null)
    {
        if (optionsAction == null)
        {
            return builder;
        }

        builder.Builder.ConfigureServices((hostBuilder, services) =>
        {
            services.PostConfigure(optionsAction);
        });

        return builder;
    }
}
