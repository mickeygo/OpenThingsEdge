namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// Tag 数据快照。
/// </summary>
internal sealed class TagDataSnapshot
{
    private readonly ConcurrentDictionary<string, PayloadDataSnapshot> _map = new();
    private long _version;

    /// <summary>
    /// 数据更改版本。清空数据后版本会重置。
    /// </summary>
    public long Version => _version;

    /// <summary>
    /// 获取标记值，若标记不存在，则返回 null。
    /// </summary>
    /// <param name="tagId">标记 Id</param>
    /// <returns></returns>
    public PayloadDataSnapshot? Get(string tagId)
    {
        _map.TryGetValue(tagId, out var snapshot);
        return snapshot;
    }

    /// <summary>
    /// 设置或更新标记值。
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="data"></param>
    public void Change(Tag tag, object data)
    {
        var data2 = PayloadData.FromTag(tag);
        data2.Value = data;
        Change(data2);
    }

    /// <summary>
    /// 设置或更新标记值。
    /// </summary>
    /// <param name="data">要设置的值</param>
    public void Change(PayloadData data)
    {
        Change(data.TagId, data);
    }

    /// <summary>
    /// 设置或更新标记值。
    /// </summary>
    /// <param name="tagId">标记 Id</param>
    /// <param name="data">要设置的值</param>
    public void Change(string tagId, PayloadData data)
    {
        Interlocked.Increment(ref _version);
        InternalChange(tagId, data);
    }

    private void InternalChange(string tagId, PayloadData data)
    {
        Interlocked.Increment(ref _version);
        _map.AddOrUpdate(tagId,
           k =>
           {
               return new PayloadDataSnapshot { Data = data };
           },
           (_, snapshot) =>
           {
               snapshot.Data.Value = data.Value;
               snapshot.UpdatedTime = DateTime.Now;
               snapshot.Version++;
               return snapshot;
           });
    }
}

public sealed class PayloadDataSnapshot
{
    [NotNull]
    public PayloadData? Data { get; init; }

    /// <summary>
    /// 数据更新时间
    /// </summary>
    public DateTime UpdatedTime { get; internal set; } = DateTime.Now;

    /// <summary>
    /// 数据更新的版本
    /// </summary>
    public long Version { get; internal set; } = 1;
}
