using ThingsEdge.Exchange.Contracts;

namespace ThingsEdge.Exchange.Engine.Snapshot;

/// <summary>
/// 标记数据快照实现。
/// </summary>
internal sealed class TagDataSnapshotImpl : ITagDataSnapshot
{
    private readonly ConcurrentDictionary<string, PayloadDataSnapshot> _map = new();

    public PayloadDataSnapshot? Get(string tagId)
    {
        _map.TryGetValue(tagId, out var snapshot);
        return snapshot;
    }

    public void Change(PayloadData data)
    {
        _map.AddOrUpdate(data.TagId,
          _ => new PayloadDataSnapshot { Data = data },
          (_, snapshot) =>
          {
              snapshot.Data = data; // 替换整个值，而不仅仅是其 Value 属性
              snapshot.UpdatedTime = DateTime.Now;
              snapshot.Version++;
              return snapshot;
          });
    }

    public void Clear()
    {
        _map.Clear();
    }
}
