using DeepEqual.Syntax;

namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// 标记数据值存储，主要用于数据比较。
/// </summary>
internal static class TagValueSet
{
    private static readonly ConcurrentDictionary<string, object> _map = new();

    /// <summary>
    /// 比较值，若值不同则写入新的值。
    /// </summary>
    /// <param name="tagId">标记 Id</param>
    /// <param name="newValue">新值</param>
    /// <returns>true 表示旧值与新写入的值相同；false 表示不同（或第一次写入）。</returns>
    public static bool CompareAndSwap(string tagId, object newValue)
    {
        // 初始状态，不包含数据。
        if (!_map.TryGetValue(tagId, out var oldState))
        {
            _map.GetOrAdd(tagId, newValue);
            return false;
        }
        
        if (oldState.IsDeepEqual(newValue))
        {
            return true;
        }

        _map.TryUpdate(tagId, newValue, oldState);

        return false;
    }

    /// <summary>
    /// 获取指定key的值，若没有找到，返回 null。
    /// </summary>
    /// <param name="tagId">标记 Id</param>
    /// <returns></returns>
    public static object? Get(string tagId)
    {
        _map.TryGetValue(tagId, out var obj);
        return obj;
    }

    /// <summary>
    /// 获取指定 key 的值。若没有找到或是强制转换异常，会返回 false。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tagId"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool TryGet<T>(string tagId, [MaybeNullWhen(false)]out T obj)
    {
        obj = default;
        if(_map.TryGetValue(tagId, out var obj2))
        {
            try
            {
                obj = (T)obj2;
                return true;
            }
            catch
            {

                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// 清空状态的缓存数据。
    /// </summary>
    public static void Clear() => _map.Clear();
}
