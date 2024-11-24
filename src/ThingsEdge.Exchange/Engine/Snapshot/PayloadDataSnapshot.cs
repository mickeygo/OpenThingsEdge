using ThingsEdge.Exchange.Contracts;

namespace ThingsEdge.Exchange.Engine.Snapshot;

/// <summary>
/// PayloadData 快照数据。
/// </summary>
public sealed class PayloadDataSnapshot
{
    [NotNull]
    public PayloadData? Data { get; internal set; }

    /// <summary>
    /// 数据更新时间
    /// </summary>
    public DateTime UpdatedTime { get; internal set; } = DateTime.Now;

    /// <summary>
    /// 数据更新的版本
    /// </summary>
    public long Version { get; internal set; } = 1L;
}
