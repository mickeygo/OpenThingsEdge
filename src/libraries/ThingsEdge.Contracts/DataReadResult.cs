namespace ThingsEdge.Contracts;

/// <summary>
/// 数据读取结果。
/// </summary>
public sealed class DataReadResult : AbstractResult
{
    /// <summary>
    /// 读取的 Tag 标记。
    /// </summary>
    [NotNull]
    public string? Tag { get; set; }

    /// <summary>
    /// 读取的数据值。
    /// </summary>
    public object? Value { get; set; }
}
