namespace ThingsEdge.Contracts;

/// <summary>
/// 加载数据上下文对象。
/// </summary>
public sealed class PayloadContext
{
    /// <summary>
    /// 加载的数据头。
    /// </summary>
    [NotNull]
    public Schema? Schema { get; set; }

    /// <summary>
    /// 加载的数据。
    /// </summary>
    [NotNull]
    public List<PayloadData>? Values { get; set; }
}
