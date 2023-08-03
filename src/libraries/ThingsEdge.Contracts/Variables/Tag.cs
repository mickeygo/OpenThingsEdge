namespace ThingsEdge.Contracts.Variables;

/// <summary>
/// 标记。
/// </summary>
public sealed class Tag
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
    [NotNull]
    public string? Address { get; init; }

    /// <summary>
    /// 数据长度。
    /// </summary>
    /// <remarks>注：普通类型默认长度设置为 0，当为数组或字符串时，需指定长度。</remarks>
    public int Length { get; init; }

    /// <summary>
    /// 数据类型。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TagDataType DataType { get; init; }

    /// <summary>
    /// 客户端访问模式，默认可读写。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ClientAccessMode ClientAccess { get; init; } = ClientAccessMode.ReadAndWrite;

    /// <summary>
    /// 扫描速率（毫秒），默认100ms。
    /// </summary>
    public int ScanRate { get; init; } = 100;

    /// <summary>
    /// 标记标识。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TagFlag Flag { get; init; }

    /// <summary>
    /// 用途分类。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TagUsage Usage { get; init; } = TagUsage.Master;

    /// <summary>
    /// 是否每次扫描后推送数据，为 true 时表示只有在数据有变化的情况下才会推送数据，默认为 <see cref="PublishMode.OnlyDataChanged"/>。
    /// </summary>
    /// <remarks>
    /// 注：仅适用 <see cref="TagFlag.Notice"/> 标记。
    /// </remarks>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PublishMode PublishMode { get; init; } = PublishMode.OnlyDataChanged;

    /// <summary>
    /// 标记要旨，可用于设置重要信息。
    /// </summary>
    [NotNull]
    public string? Keynote { get; init; } = string.Empty;

    /// <summary>
    /// 标记说明。
    /// </summary>
    [NotNull]
    public string? Description { get; init; } = string.Empty;

    /// <summary>
    /// 只有 <see cref="TagFlag.Trigger"/> 类型的标记集合，在该标记触发时集合中的标记数据也同时一起随着推送。
    /// </summary>
    /// <remarks><see cref="TagFlag.Switch"/> 类型标记，在开关为 On 状态也会筛选数据并一起推送。</remarks>
    [NotNull]
    public List<Tag>? NormalTags { get; init; } = new();

    /// <summary>
    /// 标记是否为数组对象。
    /// 当值不为 String 类型（包含 S7String 和 S7WString）且设定的长度大于 0 时，判定为数组。
    /// </summary>
    /// <returns></returns>
    public bool IsArray()
    {
        return Length > 0
           && DataType is not (TagDataType.String or TagDataType.S7String or TagDataType.S7WString);
    }
}
