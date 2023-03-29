namespace ThingsEdge.Router.Transport.MQTT;

/// <summary>
/// MQTT 响应消息。
/// </summary>
public class ResponseMQTTMessage
{
    /// <summary>
    /// 响应状态码，0表示成功。
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 错误描述。
    /// </summary>
    [NotNull]
    public string? ErrMessage { get; set; } = "";

    /// <summary>
    /// 要回写的值。
    /// </summary>
    [NotNull]
    public Dictionary<string, object>? CallbackItems { get; set; } = new();
}
