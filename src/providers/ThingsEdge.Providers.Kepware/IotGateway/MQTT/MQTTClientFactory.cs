using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace Ops.Contrib.Kepware.IotGateway.MQTT;

public static class MQTTClientFactory
{
    public static IMQTTClientProvider Create(MQTTClientOptions options)
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

        return new MQTTClientProvider(managedMqttClient, managedMqttClientOptions);
    }
}

