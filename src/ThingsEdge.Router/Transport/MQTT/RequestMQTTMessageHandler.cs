namespace ThingsEdge.Router.Transport.MQTT;

public sealed class RequestMQTTMessageHandler : IRequestHandler<RequestMQTTMessage, ResponseMQTTMessage>
{
    public Task<ResponseMQTTMessage> Handle(RequestMQTTMessage request, CancellationToken cancellationToken)
    {
        var message = request.Message;
        throw new NotImplementedException();
    }
}
