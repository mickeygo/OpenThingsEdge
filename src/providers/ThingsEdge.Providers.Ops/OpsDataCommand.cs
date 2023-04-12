using ThingsEdge.Providers.Ops.Exchange;

namespace ThingsEdge.Providers.Ops;

public sealed class OpsDataCommand : IDataCommand
{
    private readonly DriverConnectorManager _driverConnectorManager;

    public bool CanWrite => true;

    public OpsDataCommand(DriverConnectorManager driverConnectorManager)
    {
        _driverConnectorManager = driverConnectorManager;
    }

    public async Task<List<DataReadResult>> ReadAsync(DeviceSchema schema, IEnumerable<TagData> tags)
    {
        var connector = _driverConnectorManager.GetConnector(schema.DeviceName);
        if (connector == null)
        {
            return new(0);
        }

        var results = new List<DataReadResult>();
        foreach (var tag in tags)
        {
            var tag0 = new Tag
            {
                TagId = tag.TagId,
                Name = tag.Name,
                Address = tag.Address,
                Length = tag.Length,
                DataType = tag.DataType,
            };
            var (ok, data, err) = await connector.ReadAsync(tag0);
            results.Add(new DataReadResult
            {
                Code = ok ? 0 : 2,
                ErrorMessage = err,
                Tag = tag.Name,
                Value = data.Value,
            });
        }

        return results;
    }

    public async Task<DataWriteResult> WriteAsync<T>(DeviceSchema schema, TagData tag, T value)
    {
        var connector = _driverConnectorManager.GetConnector(schema.DeviceName);
        if (connector == null)
        {
            return DataWriteResult.From(tag.Name, 2);
        }

        PayloadData data = new()
        {
            TagId = tag.TagId,
            TagName = tag.Name,
            Address = tag.Address,
            Length = tag.Length,
            DataType = tag.DataType,
            Value = value
        };
        var (ok, err) = await connector.WriteAsync(data);
        if (!ok)
        {
            return DataWriteResult.From(tag.Name, 2, err);
        }

        return DataWriteResult.From(tag.Name);
    }
}
