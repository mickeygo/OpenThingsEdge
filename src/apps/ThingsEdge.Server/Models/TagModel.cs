namespace ThingsEdge.Server.Models;

/// <summary>
/// Tag 实体。
/// </summary>
public sealed class TagModel
{
    /// <summary>
    /// 全局唯一值。
    /// </summary>
    [NotNull]
    public string? TagId { get; set; }

    /// <summary>
    /// 标记名称
    /// </summary>
    [NotNull]
    public string? Name { get; set; }

    /// <summary>
    /// 地址 (字符串格式)。
    /// </summary>
    [NotNull]
    public string? Address { get; set; }

    /// <summary>
    /// 数据长度。
    /// </summary>
    /// <remarks>注：普通类型默认长度设置为 0，当为数组或字符串时，需指定长度。</remarks>
    public int Length { get; set; }

    /// <summary>
    /// 数据类型。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TagDataType DataType { get; set; }

    /// <summary>
    /// 客户端范围模式，默认可读写。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ClientAccessMode ClientAccess { get; set; }

    /// <summary>
    /// 扫描速率（毫秒），默认100ms。
    /// </summary>
    public int ScanRate { get; set; }

    /// <summary>
    /// 标记标识。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TagFlag Flag { get; set; }

    /// <summary>
    /// 用途分类。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TagUsage Usage { get; set; }

    /// <summary>
    /// 是否每次扫描后推送数据，为 true 时表示只有在数据有变化的情况下才会推送数据，默认为 <see cref="PublishMode.OnlyDataChanged"/>。
    /// </summary>
    /// <remarks>
    /// 注：仅适用 <see cref="TagFlag.Notice"/> 标记。
    /// </remarks>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PublishMode PublishMode { get; set; }

    /// <summary>
    /// 标记要旨，可用于设置重要信息。
    /// </summary>
    [NotNull]
    public string? Keynote { get; set; }

    /// <summary>
    /// 标记说明。
    /// </summary>
    [NotNull]
    public string? Description { get; set; }

    /// <summary>
    /// 只有 <see cref="TagFlag.Trigger"/> 类型的标记集合，在该标记触发时集合中的标记数据也同时一起随着推送。
    /// </summary>
    /// <remarks><see cref="TagFlag.Switch"/> 类型标记，在开关为 On 状态也会筛选数据并一起推送。</remarks>
    [NotNull]
    public List<Tag>? NormalTags { get; set; }

    /// <summary>
    /// 标记文本值
    /// </summary>
    public string? Text { get; set; }
}
