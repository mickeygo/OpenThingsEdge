namespace ThingsEdge.Router.Management;

/// <summary>
/// Tag标记数据容器，存储当前读取的标记值。
/// </summary>
public sealed class TagDataContainer
{
    private readonly ConcurrentDictionary<string, TagData> _map = new();
    private long _version;

    /// <summary>
    /// 数据更改版本。清空数据后版本会重置。
    /// </summary>
    public long Version => _version;

    /// <summary>
    /// 获取当前标记对应的数据，没有找到标记则返回 null。
    /// </summary>
    /// <param name="tagId">标记Id</param>
    /// <returns></returns>
    public TagData? Get(string tagId)
    {
        _map.TryGetValue(tagId, out var obj);
        return obj;
    }

    /// <summary>
    /// 设置标记对应的数据。写入方需确保 schema 与标记匹配。
    /// </summary>
    /// <param name="schema">标记 Schema</param>
    /// <param name="item">要设置的数据</param>
    internal void Set(Schema schema, PayloadData item)
    {
        Interlocked.Increment(ref _version);
        InternalSet(_map, schema, item);
    }

    /// <summary>
    /// 批量设置标记对应的数据。写入方需确保 schema 与标记匹配。
    /// </summary>
    /// <param name="schema">标记 Schema</param>
    /// <param name="items">要设置的数据集合</param>
    internal void Set(Schema schema, IEnumerable<PayloadData> items)
    {
        Interlocked.Increment(ref _version);
        foreach (var item in items)
        {
            InternalSet(_map, schema, item);
        }
    }

    /// <summary>
    /// 清空数据。
    /// </summary>
    public void Clear()
    {
        _version = 0;
        _map.Clear();
    }

    private static TagData InternalSet(ConcurrentDictionary<string, TagData> map, Schema schema, PayloadData item)
    {
        return map.AddOrUpdate(item.TagId,
            _ => new()
            {
                Schema = schema,
                Data = item,
            },
            (_, data0) =>
            {
                data0.Schema = schema;
                data0.Data = item;
                data0.UpdatedTime = DateTime.Now;

                return data0;
            });
    }
}

/// <summary>
/// 标记数据
/// </summary>
public sealed class TagData
{
    [NotNull]
    public Schema? Schema { get; internal set; }

    /// <summary>
    /// 数据
    /// </summary>
    [NotNull]
    public PayloadData? Data { get; internal set; }

    /// <summary>
    /// 数据更新时间
    /// </summary>
    public DateTime UpdatedTime { get; internal set; } = DateTime.Now;
}