namespace ThingsEdge.Providers.Ops.Snapshot;

internal sealed class InternalTagDataSnapshot : ITagDataSnapshot, ISingletonDependency
{
    private readonly ConcurrentDictionary<string, PayloadDataSnapshot> _map = new();
    private long _version;

    public long Version => _version;

    public PayloadDataSnapshot? Get(string tagId)
    {
        _map.TryGetValue(tagId, out var snapshot);
        return snapshot;
    }

    public void Change(PayloadData data)
    {
        Interlocked.Increment(ref _version);
        InternalChange(data.TagId, data);
    }

    public void Clear()
    {
        Interlocked.Exchange(ref _version, 0);
        _map.Clear();
    }

    private void InternalChange(string tagId, PayloadData data)
    {
        Interlocked.Increment(ref _version);
        _map.AddOrUpdate(tagId,
           _ => new PayloadDataSnapshot { Data = data },
           (_, snapshot) =>
           {
               snapshot.Data = data; // 替换整个值，而不仅仅是其 Value 属性
               snapshot.UpdatedTime = DateTime.Now;
               unchecked
               {
                   snapshot.Version++;
               }
               return snapshot;
           });
    }
}
