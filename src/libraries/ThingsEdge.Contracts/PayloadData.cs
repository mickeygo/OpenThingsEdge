namespace ThingsEdge.Contracts;

/// <summary>
/// 主体数据。
/// </summary>
public sealed class PayloadData
{
    /// <summary>
    /// 标记唯一Id。
    /// </summary>
    [NotNull]
    public string? TagId { get; set; }

    /// <summary>
    /// Tag 标记名称。
    /// </summary>
    [NotNull]
    public string? TagName { get; set; }

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
    /// 值，根据实际转换为对应的数据。
    /// </summary>
    [NotNull]
    public object? Value { get; set; }

    /// <summary>
    /// 数据类型。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DataType DataType { get; set; }

    /// <summary>
    /// 标记要旨，可用于记录重要信息。
    /// </summary>
    [NotNull]
    public string? Keynote { get; set; } = string.Empty;
}
