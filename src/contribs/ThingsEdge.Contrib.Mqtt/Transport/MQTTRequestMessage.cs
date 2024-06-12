namespace ThingsEdge.Contrib.Mqtt.Transport;

/// <summary>
/// 表示是由MQTT产生的请求数据。
/// 外部唯一处理器需要实现 <see cref="IRequestHandler{TRequest, TResponse}"/> 接口，
/// 其中 TRequest 为 <see cref="MQTTRequestMessage"/> 类型，TResponse 为 <see cref="MQTTRequestMessageResult"/> 类型。
/// </summary>
/// <param name="ClientId">唯一客户端Id。</param>
/// <param name="Topic">消息 Topic。</param>
/// <param name="Body">接收到的消息。</param>
public sealed record MQTTRequestMessage(string ClientId, string Topic, string Body) : IRequest<MQTTRequestMessageResult>;
