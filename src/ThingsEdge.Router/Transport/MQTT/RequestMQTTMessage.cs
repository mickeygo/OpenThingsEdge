using MQTTnet.Client;

namespace ThingsEdge.Router.Transport.MQTT;

public class RequestMQTTMessage : IRequest<ResponseMQTTMessage>
{
    public MqttApplicationMessageReceivedEventArgs Message { get; }

    public RequestMQTTMessage(MqttApplicationMessageReceivedEventArgs message)
    {
        Message = message;
    }
}
