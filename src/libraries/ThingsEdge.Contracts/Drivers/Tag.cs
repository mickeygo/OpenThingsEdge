namespace ThingsEdge.Contracts;

/// <summary>
/// 标记。
/// </summary>
public sealed class Tag
{
    /// <summary>
    /// 唯一值。
    /// </summary>
    public int Id { get; set; }

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
    public DataType DataType { get; set; }

    /// <summary>
    /// 客户端范围模式，默认可读写。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ClientAccessMode ClientAccess { get; set; } = ClientAccessMode.ReadAndWrite;

    /// <summary>
    /// 扫描速率（毫秒），默认100ms。
    /// </summary>
    public int ScanRate { get; set; } = 100;

    /// <summary>
    /// 标记标识。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TagFlag Flag { get; set; }

    /// <summary>
    /// 标记要旨，可用于设置重要信息。
    /// </summary>
    [NotNull]
    public string? Keynote { get; set; } = string.Empty;
   
    /// <summary>
    /// 标记说明。
    /// </summary>
    [NotNull]
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// <see cref="TagFlag.Trigger"/> 类型的标记集合，在该标记触发时集合中的标记数据也同时一起随着发送。
    /// </summary>
    [NotNull]
    public List<Tag>? NormalTags { get; set; } = new();

    /// <summary>
    /// 标记是否为数组对象。
    /// 当值不为 String 类型（包含 S7String 和 S7WString）且设定的长度大于 0 时，判定为数组。
    /// </summary>
    /// <returns></returns>
    public bool IsArray()
    {
        return Length > 0
           && DataType is not (DataType.S7String or DataType.S7String or DataType.S7WString);
    }
}
