namespace ThingsEdge.Contracts;

/// <summary>
/// 表示是由MQTT产生的请求数据。
/// 外部唯一处理器需要实现 <see cref="IRequestHandler{TRequest, TResponse}"/> 接口，
/// 其中 TRequest 为 <see cref="MQTTRequestMessage"/> 类型，TResponse 为 <see cref="MQTTRequestMessageResult"/> 类型。
/// </summary>
public sealed class MQTTRequestMessage : IRequest<MQTTRequestMessageResult>
{
    public string ClientId { get; }

    /// <summary>
    /// 消息 Topic。
    /// </summary>
    public string Topic { get; }

    /// <summary>
    /// 接收到的消息。
    /// </summary>
    public string Body { get; }

    public MQTTRequestMessage(string clientId, string topic, string body)
    {
        ClientId = clientId;
        Topic = topic;
        Body = body;
    }
}
