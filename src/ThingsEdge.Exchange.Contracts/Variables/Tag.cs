namespace ThingsEdge.Exchange.Contracts.Variables;

/// <summary>
/// 标记。
/// </summary>
/// <remarks>此处定义了 Tag 的必要属性，根据不同使用场景，可通过扩展该类定义一下自定义属性。</remarks>
public partial class Tag
{
    /// <summary>
    /// 全局唯一值。
    /// </summary>
    public string TagId { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 标记名称
    /// </summary>
    [NotNull]
    public string? Name { get; init; }

    /// <summary>
    /// 地址 (字符串格式)。
    /// </summary>
    [NotNull, JsonRequired]
    public string? Address { get; init; }

    /// <summary>
    /// 数据长度。
    /// </summary>
    /// <remarks>注：只有当数据为字符串类型或是要定义为数组时，才需要指定长度。</remarks>
    public int Length { get; init; }

    /// <summary>
    /// 数据类型。
    /// </summary>
    [JsonRequired, JsonConverter(typeof(JsonStringEnumConverter))]
    public TagDataType DataType { get; init; }

    /// <summary>
    /// 扫描速率（毫秒），默认100ms。
    /// </summary>
    /// <remarks>只有作为触发型号时才有效。</remarks>
    public int ScanRate { get; init; } = 100;

    /// <summary>
    /// 标记标识。
    /// </summary>
    /// <remarks>只有作为触发型号时才有效。</remarks>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TagFlag Flag { get; init; }

    /// <summary>
    /// 是否每次扫描后推送数据，为 true 时表示只有在数据有变化的情况下才会推送数据，默认为 <see cref="PublishMode.OnlyDataChanged"/>。
    /// </summary>
    /// <remarks>
    /// 注：仅适用 <see cref="TagFlag.Notice"/> 标记。
    /// </remarks>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PublishMode PublishMode { get; init; } = PublishMode.OnlyDataChanged;

    /// <summary>
    /// 标记显示名称。
    /// </summary>
    [NotNull]
    public string? DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// 标记身份标识，默认为 "Master"。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TagIdentity Identity { get; init; } = TagIdentity.Master;

    /// <summary>
    /// 标记分组标识，可用于定义将多个标记数据归为同一组，为空表示不进行分组。
    /// </summary>
    /// <remarks>注：分组中的数据类型要保持一致，如果是数组，组内各标记数据类型也应都为数组，且长度一致。</remarks>
    [NotNull]
    public string? Group { get; init; } = string.Empty;

    /// <summary>
    /// <see cref="TagFlag.Trigger"/> 和 <see cref="TagFlag.Notice"/> 类型的标记集合，在该标记触发时集合中的标记数据也同时一起随着推送。
    /// </summary>
    [NotNull]
    public List<Tag>? NormalTags { get; init; } = [];

    /// <summary>
    /// 标记是否为数组对象。
    /// 当值不为 String 类型（包含 S7String 和 S7WString）且设定的长度大于 0 时，会判定为数组。
    /// </summary>
    /// <returns></returns>
    public bool IsArray()
    {
        return Length > 0
           && DataType is not (TagDataType.String or TagDataType.S7String or TagDataType.S7WString);
    }
}
