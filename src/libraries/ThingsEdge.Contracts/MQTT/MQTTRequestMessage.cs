namespace ThingsEdge.Contracts.MQTT;

/// <summary>
/// 表示是由MQTT产生的请求数据。
/// 外部唯一处理器需要实现 <see cref="IRequestHandler{TRequest, TResponse}"/> 接口，
/// 其中 TRequest 为 <see cref="MQTTRequestMessage"/> 类型，TResponse 为 <see cref="MQTTRequestMessageResult"/> 类型。
/// </summary>
public sealed class MQTTRequestMessage : IRequest<MQTTRequestMessageResult>
{
    /// <summary>
    /// 唯一客户端Id。
    /// </summary>
    [NotNull]
    public string? ClientId { get; init; }

    /// <summary>
    /// 消息 Topic。
    /// </summary>
    [NotNull]
    public string? Topic { get; init; }

    /// <summary>
    /// 接收到的消息。
    /// </summary>
    [NotNull]
    public string? Body { get; init; }

    public MQTTRequestMessage(string clientId, string topic, string body)
    {
        ClientId = clientId;
        Topic = topic;
        Body = body;
    }
}
