using DeepEqual.Syntax;

namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// 数据值缓存工厂。
/// </summary>
public static class ValueCacheFactory
{
    private static readonly ConcurrentDictionary<string, object> _valueCache = new();

    /// <summary>
    /// 比较值，若值不同则写入新的值。
    /// </summary>
    /// <param name="key"></param>
    /// <param name="newValue">新值</param>
    /// <returns>true 表示状态相同；false 表示不同。</returns>
    public static bool CompareAndSwap(string key, object newValue)
    {
        // 初始状态，不包含数据。
        if (!_valueCache.TryGetValue(key, out var oldState))
        {
            _valueCache.GetOrAdd(key, newValue);
            return false;
        }

        if (oldState.IsDeepEqual(newValue))
        {
            return true;
        }

        _valueCache.TryUpdate(key, newValue, oldState);

        return false;
    }

    /// <summary>
    /// 清空状态的缓存数据。
    /// </summary>
    public static void Clear() => _valueCache.Clear();
}
