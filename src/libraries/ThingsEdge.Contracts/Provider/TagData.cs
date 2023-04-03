namespace ThingsEdge.Contracts;

/// <summary>
/// 标记数据。
/// </summary>
public sealed class TagData
{
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
    public DataType DataType { get; set; }

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

    /// <summary>
    /// 转换标记数据。
    /// </summary>
    /// <param name="tag">标记</param>
    /// <returns></returns>
    public static TagData From(Tag tag)
    {
        return new TagData
        {
            Name = tag.Name,
            Address = tag.Address,
            Length = tag.Length,
            DataType = tag.DataType,
        };
    }
}
