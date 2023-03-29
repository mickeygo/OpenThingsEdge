using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace ThingsEdge.Router.Transport.MQTT;

/// <summary>
/// MQTT Client 工厂
/// </summary>
public static class MQTTClientFactory
{
    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="options">参数选项。</param>
    /// <returns></returns>
    public static IMQTTClient Create(MQTTClientOptions options)
    {
        MqttFactory mqttFactory = new();
        var managedMqttClient = mqttFactory.CreateManagedMqttClient();
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithConnectionUri(options.ConnectionUri)
            .WithClientId(options.ClientId)
            .Build();
        var managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
            .WithClientOptions(mqttClientOptions)
            .Build();

        return new MQTTClient(managedMqttClient, managedMqttClientOptions);
    }
}

