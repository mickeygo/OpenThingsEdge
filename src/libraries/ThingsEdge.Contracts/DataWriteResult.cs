namespace ThingsEdge.Contracts;

/// <summary>
/// 数据写入结果。
/// </summary>
public sealed class DataWriteResult : AbstractResult
{
    /// <summary>
    /// 读取的 Tag 标记。
    /// </summary>
    [NotNull]
    public string? Tag { get; set; }
}
