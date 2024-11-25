using ThingsEdge.Exchange.Addresses;
using ThingsEdge.Exchange.Contracts;
using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Engine.Connectors;
using ThingsEdge.Exchange.Engine.Snapshot;

namespace ThingsEdge.Exchange.Interfaces.Impls;

internal sealed class TagReaderWriterImpl(ITagDataSnapshot tagDataSnapshot,
    IAddressFactory deviceFactory,
    DriverConnectorManager driverConnectorManager) : ITagReaderWriter
{
    public async Task<(bool ok, PayloadData? data, string? err)> ReadAsync(string deviceId, Tag tag)
    {
        var driver = driverConnectorManager.GetConnector(deviceId);
        if (driver == null)
        {
            return (false, default, "没有找到指定设备的连接驱动");
        }

        return await driver.ReadAsync(tag).ConfigureAwait(false);
    }

    public async Task<(bool ok, PayloadData? data, string? err)> ReadFromAsync(string tagId)
    {
        var (tag, deviceId) = GetTagAndDevice(tagId);
        if (tag == null)
        {
            return (false, default, "指定的标记不在标记配置中。");
        }

        return await ReadAsync(deviceId!, tag).ConfigureAwait(false);
    }

    public async Task<(bool ok, List<PayloadData>? data, string? err)> ReadMultiAsync(string deviceId, IEnumerable<Tag> tags, bool mulitple)
    {
        // 无法判断快照中是否是最新的数据，因此会直接从设备中读取。
        var driver = driverConnectorManager.GetConnector(deviceId);
        if (driver == null)
        {
            return (false, default, "没有找到指定设备的连接驱动");
        }

        return await driver.ReadMultiAsync(tags, mulitple).ConfigureAwait(false);
    }

    public async Task<(bool ok, List<PayloadData>? data, string? err)> ReadMultiFromAsync(string[] tagIds, bool allowOnce = true)
    {
        if (tagIds.Length == 0)
        {
            throw new ArgumentNullException(nameof(tagIds));
        }

        string? deviceId = null;
        List<Tag> tags = new(tagIds.Length);
        foreach (var tagId in tagIds)
        {
            var (tag, deviceId0) = GetTagAndDevice(tagId);
            if (tag == null)
            {
                return (false, default, $"指定的标记 '{tagId}' 不在标记配置中。");
            }

            if (deviceId != null && deviceId != deviceId0)
            {
                return (false, default, "要读取的多个标记集合应该只来源于一个设备配置。");
            }

            deviceId = deviceId0;
        }

        return await ReadMultiAsync(deviceId!, tags, allowOnce).ConfigureAwait(false);
    }

    public async Task<(bool ok, string? err)> WriteAsync(string deviceId, Tag tag, object data)
    {
        var driver = driverConnectorManager.GetConnector(deviceId);
        if (driver == null)
        {
            return (false, "没有找到指定设备的连接驱动");
        }

        var (ok, data2, err) = await driver.WriteAsync(tag, data, false).ConfigureAwait(false);
        if (ok)
        {
            // 更新快照
            tagDataSnapshot.Change(tag, data2!);
        }

        return (ok, err);
    }

    public async Task<(bool ok, string? err)> WriteToAsync(string tagId, object data)
    {
        var (tag, deviceId) = GetTagAndDevice(tagId);
        if (tag == null)
        {
            return (false, "指定的标记不在标记配置中。");
        }

        return await WriteAsync(deviceId!, tag, data).ConfigureAwait(false);
    }

    private (Tag? tag, string? deviceId) GetTagAndDevice(string tagId)
    {
        foreach (var device in deviceFactory.GetDevices())
        {
            var tag = device.Tags.SingleOrDefault(t => t.TagId == tagId);
            if (tag != null)
            {
                return (tag, device.DeviceId);
            }

            foreach (var tagGroup in device.TagGroups)
            {
                tag = tagGroup.Tags.SingleOrDefault(t => t.TagId == tagId);
                if (tag != null)
                {
                    return (tag, device.DeviceId);
                }

                foreach (var tag2 in tagGroup.Tags)
                {
                    tag = tag2.NormalTags.SingleOrDefault(t => t.TagId == tagId);
                    if (tag != null)
                    {
                        return (tag, device.DeviceId);
                    }
                }
            }
        }

        return (default, default);
    }
}
