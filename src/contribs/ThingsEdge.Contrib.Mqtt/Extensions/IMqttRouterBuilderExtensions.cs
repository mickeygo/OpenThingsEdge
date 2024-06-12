using ThingsEdge.Contrib.Mqtt.Forwarder;
using ThingsEdge.Contrib.Mqtt.Transport;
using ThingsEdge.Router.Forwarder;

namespace ThingsEdge.Router;

public static class IMqttRouterBuilderExtensions
{
    /// <summary>
    /// 添加 MQTT 客户端转发处理服务，其中 <see cref="TagFlag.Notice"/> 和 <see cref="TagFlag.Trigger"/> 会发布此事件。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="postDelegate">配置后更改委托</param>
    /// <param name="lifetime"><see cref="IForwarder"/> 与 <see cref="MqttClientForwarder"/> 注册的生命周期。</param>
    /// <param name="configName">MQTT Broker 配置名称</param>
    /// <returns></returns>
    public static IRouterBuilder AddMqttClientForwarder(this IRouterBuilder builder,
        Action<MQTTClientOptions>? postDelegate = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient,
        string configName = "MqttBroker")
    {
        builder.Builder.ConfigureServices((hostBuilder, services) =>
        {
            services.Configure<MQTTClientOptions>(hostBuilder.Configuration.GetSection(configName));
            if (postDelegate is not null)
            {
                services.PostConfigure(postDelegate);
            }

            services.Add(new ServiceDescriptor(typeof(IForwarder), ForworderSource.MQTT.ToString(), typeof(MqttClientForwarder), lifetime));
            ForwarderRegisterKeys.Default.Register(ForworderSource.MQTT.ToString());

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
}
