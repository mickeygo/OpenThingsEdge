namespace ThingsEdge.Providers.Kepware;

public sealed class KepwareDataCommand : IDataCommand
{
    public bool CanWrite => true;

    public KepwareDataCommand()
    {
        
    }

    public Task<List<DataReadResult>> ReadAsync(DeviceSchema schema, IEnumerable<TagData> tags)
    {
        throw new NotImplementedException();
    }

    public Task<DataWriteResult> WriteAsync<T>(DeviceSchema schema, TagData tag, T value)
    {
        throw new NotImplementedException();
    }
}
