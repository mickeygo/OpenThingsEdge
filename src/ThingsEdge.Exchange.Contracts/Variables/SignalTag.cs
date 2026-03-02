namespace ThingsEdge.Exchange.Contracts.Variables;

/// <summary>
/// 信号标记，其中可包含普通标记集合。
/// </summary>
public sealed class SignalTag : Tag
{
    /// <summary>
    /// 标记标识。
    /// </summary>
    /// <remarks>只有作为触发型号时才有效。</remarks>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TagFlag Flag { get; init; } = TagFlag.Normal;

    /// <summary>
    /// 扫描速率（毫秒），默认100ms。
    /// </summary>
    /// <remarks>只有作为触发信号时才有效。</remarks>
    public int ScanRate { get; init; } = 100;

    /// <summary>
    /// 是否每次扫描后推送数据，为 true 时表示只有在数据有变化的情况下才会推送数据，默认为 <see cref="PublishMode.OnlyDataChanged"/>。
    /// </summary>
    /// <remarks>
    /// 注：仅适用 <see cref="TagFlag.Notice"/> 标记。
    /// </remarks>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PublishMode PublishMode { get; init; } = PublishMode.OnlyDataChanged;

    /// <summary>
    /// 普通标记集合。 
    /// </summary>
    /// <remarks>
    /// 适用于 <see cref="TagFlag.Notice"/> 和 <see cref="TagFlag.Trigger"/> 类型的信号标记，在信号标记触发时普通标记数据也同时一起随着推送。
    /// </remarks>
    [NotNull]
    public List<Tag>? NormalTags { get; init; } = [];
}
