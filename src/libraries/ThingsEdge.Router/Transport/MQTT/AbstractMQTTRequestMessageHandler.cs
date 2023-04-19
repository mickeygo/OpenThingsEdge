namespace ThingsEdge.Router.Transport.MQTT;

/// <summary>
/// MQTT请求消息处理抽象类。
/// </summary>
public abstract class AbstractMQTTRequestMessageHandler : IRequestHandler<MQTTRequestMessage, MQTTRequestMessageResult>
{
    public virtual Task<MQTTRequestMessageResult> Handle(MQTTRequestMessage request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
