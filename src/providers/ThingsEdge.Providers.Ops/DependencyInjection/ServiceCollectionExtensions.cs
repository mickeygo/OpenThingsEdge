using ThingsEdge.Providers.Ops.Configuration;
using ThingsEdge.Providers.Ops.DependencyInjection;
using ThingsEdge.Providers.Ops.Exchange.Monitors;
using ThingsEdge.Providers.Ops.Internal;

namespace ThingsEdge.Router;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 OPS Provider 服务。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="optionsAction">参数选项设置</param>
    /// <param name="configName">配置节点名称，默认为 "Ops"。</param>
    /// <returns></returns>
    public static IRouterBuilder AddOpsProvider(this IRouterBuilder builder, Action<OpsOptions>? optionsAction = null, string configName = "Ops")
    {
        // 注册 EventBus 程序集
        builder.AddEventBusRegisterAssembly(typeof(ServiceCollectionExtensions).Assembly);

        builder.Builder.ConfigureServices((hostBuilder, services) =>
        {
            services.Configure<OpsConfig>(hostBuilder.Configuration.GetSection(configName));
            services.AddAutoDependencyInjection(typeof(ServiceCollectionExtensions).Assembly);
            services.AddHostedService<OpsStartupHostedService>();
        });

        // 注册监控器
        MonitorLoop.Register<HeartbeatMonitor>();
        MonitorLoop.Register<NoticeMonitor>();
        MonitorLoop.Register<TriggerMonitor>();
        MonitorLoop.Register<SwitchMonitor>();

        // 选项配置
        OpsOptions options = new();
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

