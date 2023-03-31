using MQTTnet.Client;

namespace Ops.Contrib.Kepware.IotGateway.MQTT;

public interface IMQTTApplicationClient
{
    Task ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args);

    Task DisconnectedAsync(MqttClientDisconnectedEventArgs args);
}
