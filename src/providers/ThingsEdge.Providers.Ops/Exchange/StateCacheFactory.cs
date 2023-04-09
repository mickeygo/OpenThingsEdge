namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// 状态缓存工厂。
/// </summary>
public static class StateCacheFactory
{
    private static readonly ConcurrentDictionary<string, int> _stateCache = new();

    /// <summary>
    /// 比较状态并交互缓存中的值。
    /// </summary>
    /// <param name="key"></param>
    /// <param name="newState"></param>
    /// <returns>true 表示状态相同；false 表示不同。</returns>
    public static bool CompareAndSwap(string key, int newState)
    {
        // 初始状态，不包含数据。
        if(!_stateCache.TryGetValue(key, out int oldState))
        {
            _stateCache.GetOrAdd(key, newState);
            return false;
        }

        _stateCache.TryUpdate(key, newState, oldState);

        return oldState != newState;
    }

    /// <summary>
    /// 清空状态的缓存数据。
    /// </summary>
    public static void Clear() => _stateCache.Clear();
}
