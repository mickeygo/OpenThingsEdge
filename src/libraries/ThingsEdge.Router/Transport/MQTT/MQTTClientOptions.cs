namespace ThingsEdge.Router.Transport.MQTT;

public sealed class MQTTClientOptions
{
    /// <summary>
    /// MQTT 服务器连接地址，如：mqtt://user:password@127.0.0.1，其中scheme可为tcp、mqtt、mqtts、ws和wss。
    /// </summary>
    [NotNull]
    public string? ConnectionUri { get; set; }

    /// <summary>
    /// 客户端唯一标识，默认为 "ThingsEdge"。
    /// </summary>
    [NotNull]
    public string? ClientId { get; set; } = "ThingsEdge.Router";

    /// <summary>
    /// 允许本地存储最大消息数量，默认为 <see cref="short.MaxValue"/>。
    /// </summary>
    public int MaxPendingMessages { get; set; } = short.MaxValue;

    /// <summary>
    /// MQTT 协议版本，默认为 3.1.1 。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MQTTnet.Formatter.MqttProtocolVersion ProtocolVersion { get; set; } = MQTTnet.Formatter.MqttProtocolVersion.V311;
}
