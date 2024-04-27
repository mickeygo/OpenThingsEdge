using ThingsEdge.Providers.Ops.Snapshot;
using ThingsEdge.Router.Devices;

namespace ThingsEdge.Providers.Ops.Exchange;

internal sealed class DeviceReadWriteImpl : IDeviceReadWrite, ISingletonDependency
{
    private readonly ITagDataSnapshot _tagDataSnapshot;
    private readonly DriverConnectorManager _driverConnectorManager;

    public DeviceReadWriteImpl(ITagDataSnapshot tagDataSnapshot, DriverConnectorManager driverConnectorManager)
    {
        _tagDataSnapshot = tagDataSnapshot;
        _driverConnectorManager = driverConnectorManager;
    }

    public async Task<DeviceReadResult> ReadAsync(string deviceId, IEnumerable<Tag> tags)
    {
        DeviceReadResult result = new()
        {
            Data = [],
        };

        List<Tag> tags2 = [];

        // 先从快照中读取
        // TODO: 如何判断快照中数据是不是最新的？
        foreach (var tag in tags)
        {
            var snapshot = _tagDataSnapshot.Get(tag.TagId);
            if (snapshot != null)
            {
                result.Data.Add(snapshot.Data);
            }
            else
            {
                tags2.Add(tag);
            }
        }

        if (tags2.Count == 0)
        {
            return result;
        }

        // 若存在快照中没有的数据，继续从设备中读取
        // 在设备不可用或读取出错时，直接返回异常。
        var driver = _driverConnectorManager.GetConnector(deviceId);
        if (driver == null)
        {
            result.Code = 2;
            result.ErrorMessage = "没有找到对应设备的驱动";
            return result;
        }

        var (ok, data, err) = await driver.ReadMultiAsync(tags2, false).ConfigureAwait(false);
        if (ok)
        {
            // 更新快照值
            _tagDataSnapshot.Change(data!);
            result.Data.AddRange(data!);
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

        var (ok, data2, err) = await driver.WriteAsync(tag, data, false).ConfigureAwait(false);
        if (ok)
        {
            // 更新快照
            _tagDataSnapshot.Change(tag, data2!);
        }

        return (ok, err);
    }
}
