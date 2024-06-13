using ThingsEdge.Contrib.Mqtt.Forwarders;
using ThingsEdge.Contrib.Mqtt.Transport;
using ThingsEdge.Router.Forwarders;

namespace ThingsEdge.Router;

public static class IMqttRouterBuilderExtensions
{
    /// <summary>
    /// 添加 MQTT 客户端数据发送服务，其中 <see cref="TagFlag.Trigger"/> 会发布此事件。
    /// </summary>
    /// <typeparam name="TForwarder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="postDelegate">配置后更改委托</param>
    /// <param name="lifetime"></param>
    /// <param name="configName">MQTT Broker 配置名称</param>
    /// <returns></returns>
    public static IRouterBuilder AddMqttTriggerForwarder<TForwarder>(this IRouterBuilder builder,
        Action<MQTTClientOptions>? postDelegate = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient,
        string configName = "MqttBroker")
        where TForwarder : IRequestForwarder
    {
        builder.Builder.ConfigureServices((hostBuilder, services) =>
        {
            services.Configure<MQTTClientOptions>(hostBuilder.Configuration.GetSection(configName));
            if (postDelegate is not null)
            {
                services.PostConfigure(postDelegate);
            }

            services.Add(ServiceDescriptor.DescribeKeyed(typeof(IRequestForwarder), "MQTT", typeof(TForwarder), lifetime));
            ForwarderRegisterHub.Default.Register("MQTT");

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
    /// 添加 MQTT 客户端数据发送服务，其中 <see cref="TagFlag.Trigger"/> 会发布此事件。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="postDelegate">配置后更改委托</param>
    /// <param name="lifetime"></param>
    /// <param name="configName">MQTT Broker 配置名称</param>
    /// <returns></returns>
    public static IRouterBuilder AddMqttTriggerForwarder(this IRouterBuilder builder,
        Action<MQTTClientOptions>? postDelegate = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient,
        string configName = "MqttBroker")
    {
        return builder.AddMqttTriggerForwarder<MqttRequestForwarder>(postDelegate, lifetime, configName);
    }
}
