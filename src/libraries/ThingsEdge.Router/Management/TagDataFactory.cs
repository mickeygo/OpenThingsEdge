namespace ThingsEdge.Router.Management;

/// <summary>
/// Tag标记数据工厂，记录标记读取的值。
/// </summary>
public sealed class TagDataFactory
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
    /// <param name="data">要设置的数据</param>
    internal void Set(PayloadData data)
    {
        Interlocked.Increment(ref _version);
        _map.AddOrUpdate(data.TagId,
            _ => new() { Data = data },
            (_, data0) =>
            {
                data0.Data = data;
                data0.UpdatedTime = DateTime.Now;

                return data0;
            });
    }

    /// <summary>
    /// 批量设置标记对应的数据。
    /// </summary>
    /// <param name="datas"></param>
    internal void Set(IEnumerable<PayloadData> datas)
    {
        Interlocked.Increment(ref _version);
        foreach (var data in datas)
        {
            _map.AddOrUpdate(data.TagId,
                _ => new() { Data = data },
                (_, data0) =>
                {
                    data0.Data = data;
                    data0.UpdatedTime = DateTime.Now;

                    return data0;
                });
        }
    }

    /// <summary>
    /// 获取标记对应的数据，没有找到标记则返回 null。
    /// </summary>
    /// <param name="tagId">标记Id</param>
    /// <returns></returns>
    public TagData? Get(string tagId)
    {
        _map.TryGetValue(tagId, out var obj);
        return obj;
    }

    /// <summary>
    /// 清空数据。
    /// </summary>
    public void Clear()
    {
        _map.Clear();
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
        public PayloadData? Data { get; set; }

        /// <summary>
        /// 数据更新时间
        /// </summary>
        public DateTime UpdatedTime { get; set; } = DateTime.Now;
    }
}
