namespace ThingsEdge.Contrib.Mqtt.Transport;

public sealed class MQTTClientTopicFormaterArg
{
    /// <summary>
    /// 加载的数据头。
    /// </summary>
    [NotNull]
    public Schema? Schema { get; init; }

    public TagFlag Flag { get; init; }

    [NotNull]
    public string? TagName { get; init; }
}
