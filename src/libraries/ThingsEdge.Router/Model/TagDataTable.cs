namespace ThingsEdge.Router.Model;

/// <summary>
/// 数据标记表，用于存储标记当前的数据。
/// </summary>
public sealed class TagDataTable
{
    private readonly ConcurrentDictionary<string, PayloadData> _map = new();
    private long _version;

    /// <summary>
    /// 设置标记对应的数据。
    /// </summary>
    /// <param name="tagId">标记Id</param>
    /// <param name="data">要设置的数据</param>
    public void Set(string tagId, PayloadData data)
    {
        Interlocked.Increment(ref _version);
        _map.AddOrUpdate(tagId, data, (k, v) => data);
    }

    /// <summary>
    /// 获取标记对应的数据，没有找到标记则返回 null。
    /// </summary>
    /// <param name="tagId">标记Id</param>
    /// <returns></returns>
    public PayloadData? Get(string tagId)
    {
        _map.TryGetValue(tagId, out var obj);
        return obj;
    }
}
