namespace ThingsEdge.Contracts.MQTT;

/// <summary>
/// 表示是MQTT产生的请求数据处理后返回的数据。
/// </summary>
public sealed class MQTTRequestMessageResult : AbstractResult
{
    /// <summary>
    /// 处理好的请求消息。
    /// </summary>
    [NotNull]
    public RequestMessage? RequestMessage { get; init; }

    /// <summary>
    /// Schema
    /// </summary>
    [NotNull]
    public List<Schema>? Schemas { get; init; }

    /// <summary>
    /// 是否要自动回执。
    /// </summary>
    public bool AutoAcknowledge { get; set; }
}
