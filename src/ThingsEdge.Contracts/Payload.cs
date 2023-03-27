namespace ThingsEdge.Contracts;

/// <summary>
/// 主体数据。
/// </summary>
public sealed class Payload
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
    /// 时间戳
    /// </summary>
    public long Timestamp { get; set; }
}
