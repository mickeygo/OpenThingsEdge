namespace ThingsEdge.Providers.Ops;

public sealed class OpsDataCommand : IDataCommand
{
    public bool CanWrite => true;

    public OpsDataCommand()
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
