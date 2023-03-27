using MQTTnet.Client;

namespace ThingsEdge.Router.Transport.MQTT;

public interface IMQTTApplicationClient
{
    Task ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args);

    Task DisconnectedAsync(MqttClientDisconnectedEventArgs args);
}
