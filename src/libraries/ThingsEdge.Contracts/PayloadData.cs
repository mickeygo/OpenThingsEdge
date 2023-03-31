namespace ThingsEdge.Contracts;

/// <summary>
/// 主体数据。
/// </summary>
public sealed class PayloadData
{
    /// <summary>
    /// Tag 标签
    /// </summary>
    [NotNull]
    public string? Tag { get; set; }

    /// <summary>
    /// 值，根据实际转换为对应的数据。
    /// </summary>
    [NotNull]
    public object? Value { get; set; }

    /// <summary>
    /// 数据类型。
    /// </summary>
    public DataType DataType { get; set; }

    /// <summary>
    /// 标记要旨，可用于记录重要信息。
    /// </summary>
    [NotNull]
    public string? Keynote { get; set; } = string.Empty;
}
