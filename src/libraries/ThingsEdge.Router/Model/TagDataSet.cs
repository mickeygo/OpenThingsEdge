namespace ThingsEdge.Router.Model;

/// <summary>
/// 标记数据集合，用于存储标记当前的数据。
/// </summary>
public sealed class TagDataSet
{
    private readonly ConcurrentDictionary<string, TagData> _map = new();
    private long _version;

    /// <summary>
    /// 数据更改版本。
    /// </summary>
    public long Version => _version;

    /// <summary>
    /// 设置标记对应的数据。
    /// </summary>
    /// <param name="tagId">标记Id</param>
    /// <param name="data">要设置的数据</param>
    internal void Set(string tagId, PayloadData data)
    {
        Interlocked.Increment(ref _version);
        TagData data1 = new() { Data = data };
        _map.AddOrUpdate(tagId, data1, (_, _) => data1);
    }

    /// <summary>
    /// 获取标记对应的数据，没有找到标记则返回 null。
    /// </summary>
    /// <param name="tagId">标记Id</param>
    /// <returns></returns>
    internal TagData? Get(string tagId)
    {
        _map.TryGetValue(tagId, out var obj);
        return obj;
    }
}

/// <summary>
/// 标记数据
/// </summary>
public sealed class TagData
{
    /// <summary>
    /// 数据
    /// </summary>
    [NotNull]
    public PayloadData? Data { get; init; }

    /// <summary>
    /// 数据更新时间
    /// </summary>
    public DateTime UpdatedTime { get; init; } = DateTime.Now;
}
