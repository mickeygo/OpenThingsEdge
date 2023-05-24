using ThingsEdge.Router.Devices;

namespace ThingsEdge.Providers.Ops.Exchange;

internal sealed class OpsDeviceReadWrite : IDeviceReadWrite
{
    private readonly TagDataSnapshot _tagDataSnapshot;
    private readonly DriverConnectorManager _driverConnectorManager;

    public OpsDeviceReadWrite(TagDataSnapshot tagDataSnapshot, DriverConnectorManager driverConnectorManager)
    {
        _tagDataSnapshot = tagDataSnapshot;
        _driverConnectorManager = driverConnectorManager;
    }

    public async Task<DeviceReadResult> ReadAsync(string deviceId, IEnumerable<Tag> tags)
    {
        DeviceReadResult result = new();

        // 从快照中读取
        var driver = _driverConnectorManager.GetConnector(deviceId);
        if (driver == null)
        {
            result.Code = 2;
            result.ErrorMessage = "没有找到对应设备的驱动";
            return result;
        }

        var (ok, data, err) = await driver.ReadMultiAsync(tags).ConfigureAwait(false);
        if (ok)
        {
            result.Data = data;
        }
        else
        {
            result.Code = 2;
            result.ErrorMessage = err;
        }

        return result;
    }

    public async Task<(bool ok, string? err)> WriteAsync(string deviceId, Tag tag, object data)
    {
        var driver = _driverConnectorManager.GetConnector(deviceId);
        if (driver == null)
        {
            return (false, "没有找到对应设备的驱动");
        }

        // 更新快照

        return await driver.WriteAsync(tag, data, false).ConfigureAwait(false);
    }
}
