using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace ThingsEdge.Router.Transport.MQTT;

/// <summary>
/// MQTT 客户端工厂。
/// </summary>
public static class MQTTClientFactory
{
    /// <summary>
    /// 创建 MQTT 客户端。
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IMQTTClient CreateClient(MQTTClientOptions options)
    {
        var mqttClientOptions = BuildMqttClientOptions(options);

        MqttFactory mqttFactory = new();
        var mqttClient = mqttFactory.CreateMqttClient();
        return new MQTTClient(mqttClient, mqttClientOptions);
    }

    /// <summary>
    /// 创建托管的 MQTT 客户端。托管客户启动后，会不间断尝试去连接 Broker，且消息可以存储在内部的队列中，一旦客户端连接成功会立即发送。
    /// </summary>
    /// <param name="options">参数选项。</param>
    /// <returns></returns>
    public static IMQTTManagedClient CreateManagedClient(MQTTClientOptions options)
    {
        var mqttClientOptions = BuildMqttClientOptions(options);
        var managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
            .WithClientOptions(mqttClientOptions)
            .WithMaxPendingMessages(options.MaxPendingMessages)
            .Build();

        MqttFactory mqttFactory = new();
        var managedMqttClient = mqttFactory.CreateManagedMqttClient();
        return new MQTTManagedClient(managedMqttClient, managedMqttClientOptions);
    }

    private static MqttClientOptions BuildMqttClientOptions(MQTTClientOptions options)
        => new MqttClientOptionsBuilder()
            .WithConnectionUri(options.ConnectionUri)
            .WithClientId(options.ClientId)
            .WithProtocolVersion(options.ProtocolVersion)
            .Build();
}

