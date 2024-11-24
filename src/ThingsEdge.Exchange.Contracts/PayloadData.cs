using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Contracts;

/// <summary>
/// 主体数据。
/// </summary>
public sealed class PayloadData
{
    /// <summary>
    /// 标记唯一Id。
    /// </summary>
    [NotNull]
    public string? TagId { get; init; }

    /// <summary>
    /// Tag 标记名称。
    /// </summary>
    [NotNull]
    public string? TagName { get; init; }

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
    /// 值，根据实际转换为对应的数据。
    /// </summary>
    [NotNull]
    public object? Value { get; set; }

    /// <summary>
    /// 数据类型。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TagDataType DataType { get; init; }

    /// <summary>
    /// 标记显示名称。
    /// </summary>
    [NotNull]
    public string? DisplayName { get; init; }

    /// <summary>
    /// 数据身份标记。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TagIdentity Identity { get; init; }

    /// <summary>
    /// 标记分组标识，可用于定义将多个标记数据归为同一组，为空表示不进行分组。
    /// </summary>
    /// <remarks>注：分组中的数据类型要保持一致，如果是数组，组内各标记数据类型也应都为数组，且长度一致。</remarks>
    [NotNull]
    public string? Group { get; init; }

    /// <summary>
    /// 标记值的用途标识
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TagValueUsage ValueUsage { get; init; }

    /// <summary>
    /// 数据创建时间，可用于诊断。
    /// </summary>
    public DateTime CreatedTime { get; init; } = DateTime.Now;

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

    /// <summary>
    /// 复制 Tag 数据到此对象。
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static PayloadData FromTag(Tag tag)
    {
        return new PayloadData
        {
            TagId = tag.TagId,
            TagName = tag.Name,
            Address = tag.Address,
            DataType = tag.DataType,
            Length = tag.Length,
            DisplayName = tag.DisplayName,
            Identity = tag.Identity,
            Group = tag.Group,
            ValueUsage = tag.ValueUsage,
        };
    }
}
