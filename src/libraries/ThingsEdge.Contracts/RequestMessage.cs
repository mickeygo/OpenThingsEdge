namespace ThingsEdge.Contracts;

/// <summary>
/// 请求消息。
/// </summary>
public sealed class RequestMessage
{
    /// <summary>
    /// 请求消息的 Id，可用于追踪。
    /// </summary>
    [NotNull]
    public string RequestId { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 加载的数据头。
    /// </summary>
    [NotNull]
    public Schema? Schema { get; init; }

    /// <summary>
    /// 标识。
    /// </summary>
    public TagFlag Flag { get; init; }

    /// <summary>
    /// 加载的数据集合。
    /// </summary>
    /// <remarks>注：加载的标记数据集合会包含触发的标记数据。</remarks>
    [NotNull]
    public List<PayloadData>? Values { get; } = new();

    /// <summary>
    /// 通过标记获取指定是加载数据，如果没有找到则返回 null。
    /// </summary>
    /// <param name="tagName">标记名称，不区分大小写</param>
    /// <returns></returns>
    public PayloadData? GetData(string tagName)
    {
        return Values!.FirstOrDefault(x => tagName.Equals(x.TagName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 获取标记触发点的数据。
    /// </summary>
    /// <returns></returns>
    public PayloadData Self()
    {
        return Values[0];
    }

    /// <summary>
    /// 获取除标记触发点以外子集合的所有数据。
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<PayloadData> Children()
    {
        if (Values.Count == 1)
        {
            return Array.Empty<PayloadData>();
        }
        return Values.Skip(1).ToList();
    }
}
