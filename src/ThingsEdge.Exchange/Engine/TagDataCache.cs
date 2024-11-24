using ThingsEdge.Exchange.Utils;

namespace ThingsEdge.Exchange.Engine;

/// <summary>
/// 标记数据缓存对象，主要用于数据比较。
/// </summary>
internal static class TagDataCache
{
    private static readonly ConcurrentDictionary<string, InternalData> s_dataMap = new();

    /// <summary>
    /// 比较值，若新值与旧值不同或旧值不存在，则写入新的值。
    /// </summary>
    /// <param name="tagId">标记 Id</param>
    /// <param name="newValue">新值</param>
    /// <param name="checkAckWhenEqual">当新值与旧值相等时，是否检测是否有 Ack 设置。</param>
    /// <returns>
    /// <para>第一次写入时，返回 false；</para>
    /// <para>旧值与新值相同时，返回 true，否则返回 false；</para>
    /// <para>
    /// 旧值与新值若不同，不检查 ack 时，直接返回 false；
    /// 若相同，且设置了开启检查 ack，有 ack 处理则返回 false，否则返回 true。
    /// 执行函数后 ack 会重置为 false 状态。
    /// </para>
    /// </returns>
    public static bool CompareAndSwap(string tagId, object newValue, bool checkAckWhenEqual = false)
    {
        // 初始状态，不包含数据。
        if (!s_dataMap.TryGetValue(tagId, out var data))
        {
            _ = s_dataMap.GetOrAdd(tagId, new InternalData { Value = newValue });
            return false;
        }

        // 始终重置回执状态
        var ack = data.Ack;
        if (ack)
        {
            data.Ack = false;
        }

        // 新旧值比较相等
        if (ObjectEqualUtils.IsEqual(newValue, data.Value))
        {
            data.Version++;

            return !checkAckWhenEqual || !ack;
        }

        // 新旧值比较不相等，更改对象的值（不替换 value）
        data.Value = newValue;
        data.Version = 1;
        data.AckVersion = 0;

        return false;
    }

    /// <summary>
    /// 回执数据。
    /// </summary>
    /// <param name="tagId">标记 Id</param>
    /// <param name="maxAckVersion">允许的最大回执版本号，当值版本号超过设置的后，不会再设置 Ack 为 true，默认为 null。</param>
    public static void Ack(string tagId, int? maxAckVersion = null)
    {
        if (s_dataMap.TryGetValue(tagId, out var data))
        {
            ++data.AckVersion;
            if (maxAckVersion is not null)
            {
                if (data.AckVersion <= maxAckVersion)
                {
                    data.Ack = true;
                }
            }
            else
            {
                data.Ack = true;
            }
        }
    }

    /// <summary>
    /// 清空状态的缓存数据。
    /// </summary>
    public static void Clear() => s_dataMap.Clear();

    class InternalData
    {
        [NotNull]
        public object? Value { get; set; }

        /// <summary>
        /// 是否有回执，默认 false。
        /// </summary>
        public bool Ack { get; set; }

        /// <summary>
        /// 已设置 Ack 的次数。
        /// </summary>
        public int AckVersion { get; set; }

        /// <summary>
        /// 相同值比较次数，值更新后版本重置为 1
        /// </summary>
        public long Version { get; set; } = 1;
    }
}
