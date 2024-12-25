namespace ThingsEdge.Exchange.Contracts;

/// <summary>
/// 请求消息。
/// </summary>
public sealed class RequestMessage
{
    /// <summary>
    /// 请求消息的 Id，用于追踪。
    /// </summary>
    public string RequestId { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 消息请求的时间。
    /// </summary>
    public DateTime RequestTime { get; init; } = DateTime.Now;

    /// <summary>
    /// 加载的数据概要。
    /// </summary>
    [NotNull]
    public Schema? Schema { get; init; }

    /// <summary>
    /// 加载的数据集合。
    /// </summary>
    /// <remarks>注：加载的标记数据集合会包含触发点的信号标记数据。</remarks>
    [NotNull]
    public List<PayloadData>? Values { get; init; } = [];

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
    /// <remarks>通知数据和触发数据为信号标记值；心跳事件没有触发点值，调用后会出现异常。</remarks>
    /// <returns></returns>
    public PayloadData Self()
    {
        return Values[0];
    }
}
