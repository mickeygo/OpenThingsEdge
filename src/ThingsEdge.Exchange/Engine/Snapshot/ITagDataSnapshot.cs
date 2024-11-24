using ThingsEdge.Exchange.Contracts;

namespace ThingsEdge.Exchange.Engine.Snapshot;

/// <summary>
/// 标记数据快照。
/// </summary>
public interface ITagDataSnapshot
{
    /// <summary>
    /// 获取标记值，若标记不存在，则返回 null。
    /// </summary>
    /// <param name="tagId">标记 Id</param>
    /// <returns></returns>
    PayloadDataSnapshot? Get(string tagId);

    /// <summary>
    /// 设置或更新标记值。
    /// </summary>
    /// <param name="data"></param>
    void Change(PayloadData data);

    /// <summary>
    /// 清除所有数据。
    /// </summary>
    void Clear();
}
