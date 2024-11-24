using ThingsEdge.Exchange.Contracts;
using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Engine.Snapshot;

public static class ITagDataSnapshotExtensions
{
    /// <summary>
    /// 设置或更新标记值快照。
    /// </summary>
    /// <param name="snapshot"></param>
    /// <param name="tag">标记</param>
    /// <param name="data">要设置的数据</param>
    public static void Change(this ITagDataSnapshot snapshot, Tag tag, object data)
    {
        var data2 = PayloadData.FromTag(tag);
        data2.Value = data;
        snapshot.Change(data2);
    }

    /// <summary>
    /// 设置或更新标记值快照。
    /// </summary>
    /// <param name="snapshot"></param>
    /// <param name="data">要设置的数据</param>
    public static void Change(this ITagDataSnapshot snapshot, IEnumerable<PayloadData> data)
    {
        foreach (var data2 in data)
        {
            snapshot.Change(data2);
        }
    }
}
