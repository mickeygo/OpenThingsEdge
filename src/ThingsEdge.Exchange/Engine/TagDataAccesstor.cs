namespace ThingsEdge.Exchange.Engine;

/// <summary>
/// 标记数据存取器，主要用于数据比较。
/// </summary>
internal static class TagDataAccesstor
{
    private static readonly ConcurrentDictionary<string, TagDataPlaceholder> s_dataMap = new();

    /// <summary>
    /// 比较缓存中 Tag 值与指定值是否相等，若不同等时会用新值替换。
    /// </summary>
    /// <param name="tagId">标记 Id</param>
    /// <param name="newValue">新值</param>
    /// <returns>
    /// 新值与旧值不等时返回 true，相等时返回 false；Tag 值不存在时（第一次比较）返回 true。
    /// </returns>
    public static bool CompareAndExchange(string tagId, object newValue)
    {
        // 初始状态，不包含数据。
        if (!s_dataMap.TryGetValue(tagId, out var data))
        {
            _ = s_dataMap.GetOrAdd(tagId, new TagDataPlaceholder { Value = newValue, Version = 1 });
            return true;
        }

        // 新旧值比较不等时
        if (!ObjectComparator.IsEqual(data.Value, newValue))
        {
            data.Value = newValue;
            data.Version++;

            return true;
        }

        return false;
    }

    /// <summary>
    /// 获取标记的值。
    /// </summary>
    /// <param name="tagId">标记 Id</param>
    /// <returns></returns>
    public static object? Get(string tagId)
    {
        if (s_dataMap.TryGetValue(tagId, out var data))
        {
            return data.Value;
        }

        return null;
    }

    /// <summary>
    /// 清空状态的缓存数据。
    /// </summary>
    public static void Clear() => s_dataMap.Clear();

    private sealed class TagDataPlaceholder
    {
        /// <summary>
        /// 值
        /// </summary>
        [NotNull]
        public object? Value { get; set; }

        /// <summary>
        /// 比较并更换值的次数。
        /// </summary>
        public long Version { get; set; }
    }
}
