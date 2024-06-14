using ThingsEdge.Providers.Ops.Exchange;
using ThingsEdge.Providers.Ops.Snapshot;
using ThingsEdge.Router.Interfaces;

namespace ThingsEdge.Providers.Ops.Impls;

internal sealed class TagReaderWriterImpl : ITagReaderWriter, ISingletonDependency
{
    private readonly ITagDataSnapshot _tagDataSnapshot;
    private readonly DriverConnectorManager _driverConnectorManager;

    public TagReaderWriterImpl(ITagDataSnapshot tagDataSnapshot, DriverConnectorManager driverConnectorManager)
    {
        _tagDataSnapshot = tagDataSnapshot;
        _driverConnectorManager = driverConnectorManager;
    }

    public async Task<(bool ok, PayloadData? data, string? err)> ReadAsync(string deviceId, Tag tag)
    {
        var driver = _driverConnectorManager.GetConnector(deviceId);
        if (driver == null)
        {
            return (false, default, "没有找到指定设备的连接驱动");
        }

        return await driver.ReadAsync(tag);
    }

    public async Task<(bool ok, List<PayloadData>? data, string? err)> ReadAsync(string deviceId, IEnumerable<Tag> tags, bool mulitple)
    {
        // 无法判断快照中是否是最新的数据，因此会直接从设备中读取。
        var driver = _driverConnectorManager.GetConnector(deviceId);
        if (driver == null)
        {
            return (false, default, "没有找到指定设备的连接驱动");
        }

        return await driver.ReadMultiAsync(tags, mulitple).ConfigureAwait(false);
    }

    public async Task<(bool ok, string? err)> WriteAsync(string deviceId, Tag tag, object data)
    {
        var driver = _driverConnectorManager.GetConnector(deviceId);
        if (driver == null)
        {
            return (false, "没有找到指定设备的连接驱动");
        }

        var (ok, data2, err) = await driver.WriteAsync(tag, data, false).ConfigureAwait(false);
        if (ok)
        {
            // 更新快照
            _tagDataSnapshot.Change(tag, data2!);
        }

        return (ok, err);
    }
}
