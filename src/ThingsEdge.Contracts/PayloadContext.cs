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
    /// 加载的数据集合。
    /// </summary>
    /// <remarks>注：加载的标记数据集合会包含触发的标记数据。</remarks>
    [NotNull]
    public List<PayloadData>? Values { get; set; }

    /// <summary>
    /// 通过标记获取指定是加载数据，如果没有找到则返回 null。
    /// </summary>
    /// <param name="tag">标记，不区分大小写</param>
    /// <returns></returns>
    public PayloadData? GetData(string tag)
    {
        return Values!.FirstOrDefault(x => tag.Equals(x.Tag, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 获取标记触发点的数据。
    /// </summary>
    /// <returns></returns>
    public PayloadData Self()
    {
        return Values[0];
    }
}
