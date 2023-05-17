using DeepEqual.Syntax;

namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// 标记数据存储集合。
/// </summary>
internal static class TagValueSet
{
    private static readonly ConcurrentDictionary<string, object> _table = new();

    /// <summary>
    /// 比较值，若值不同则写入新的值。
    /// </summary>
    /// <param name="key"></param>
    /// <param name="newValue">新值</param>
    /// <returns>true 表示旧值与新写入的值相同；false 表示不同（或第一次写入）。</returns>
    public static bool CompareAndSwap(string key, object newValue)
    {
        // 初始状态，不包含数据。
        if (!_table.TryGetValue(key, out var oldState))
        {
            _table.GetOrAdd(key, newValue);
            return false;
        }
        
        if (oldState.IsDeepEqual(newValue))
        {
            return true;
        }

        _table.TryUpdate(key, newValue, oldState);

        return false;
    }

    /// <summary>
    /// 获取指定key的值，若没有找到，返回 null。
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static object? Get(string key)
    {
        _table.TryGetValue(key, out var obj);
        return obj;
    }

    /// <summary>
    /// 清空状态的缓存数据。
    /// </summary>
    public static void Clear() => _table.Clear();
}
