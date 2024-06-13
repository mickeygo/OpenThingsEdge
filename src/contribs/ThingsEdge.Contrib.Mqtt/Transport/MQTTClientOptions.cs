namespace ThingsEdge.Contrib.Mqtt.Transport;

/// <summary>
/// MQTT 客户端选项
/// </summary>
public sealed class MQTTClientOptions
{
    /// <summary>
    /// MQTT 服务器连接地址。
    /// </summary>
    [NotNull]
    public string? ServerUri { get; set; }

    /// <summary>
    /// MQTT 服务器连接地址，为 null 表示使用默认端口。
    /// </summary>
    public int? Port { get; set; }

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
    /// Topic 格式器，系统内部默认会采用 {ChannelName}/{DeviceName}/{TagGroupName} 模式匹配，匹配规则不区分大小写。
    /// </summary>
    /// <remarks>
    /// Topic 格式：
    ///   {ChannelName}/{DeviceName}/{TagGroupName} => line01/device01/op010, 
    ///   iot/{ChannelName}/{DeviceName} => iot/line01/device01 。
    /// 若占位符没有找到值，会移除占位符。
    /// </remarks>
    public string? TopicFormater { get; set; }

    /// <summary>
    /// Topic 格式化时匹配的数据是否要转为小写，默认为 true。
    /// </summary>
    /// <remarks>topic 是区分大小写的。</remarks>
    public bool TopicFormatMatchLower { get; set; } = true;

    /// <summary>
    /// MQTT Topic 格式器，若设定了该函数，会替代默认的格式器。
    /// </summary>
    [JsonIgnore]
    public Func<MQTTClientTopicFormaterArg, string>? TopicFormaterFunc { get; set; }

    /// <summary>
    /// MQTT Payload 格式器，设定了该函数后会调用。
    /// </summary>
    [JsonIgnore]
    public Func<RequestMessage, string>? PayloadFormaterFunc { get; set; }

    /// <summary>
    /// MQTT 协议版本，默认为 3.1.1 。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MQTTnet.Formatter.MqttProtocolVersion ProtocolVersion { get; set; } = MQTTnet.Formatter.MqttProtocolVersion.V311;

    /// <summary>
    /// 推送成功后默认返回的状态码。
    /// </summary>
    public int SuccessCode { get; set; }
}
