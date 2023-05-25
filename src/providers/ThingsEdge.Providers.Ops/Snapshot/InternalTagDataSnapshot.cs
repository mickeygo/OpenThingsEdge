namespace ThingsEdge.Providers.Ops.Snapshot;

internal sealed class InternalTagDataSnapshot : ITagDataSnapshot
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
           k =>
           {
               return new PayloadDataSnapshot { Data = data };
           },
           (_, snapshot) =>
           {
               snapshot.Data.Value = data.Value;
               snapshot.UpdatedTime = DateTime.Now;
               snapshot.Version++;
               return snapshot;
           });
    }
}
