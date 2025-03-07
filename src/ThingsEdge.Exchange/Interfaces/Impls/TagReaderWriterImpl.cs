using ThingsEdge.Exchange.Addresses;
using ThingsEdge.Exchange.Contracts;
using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Engine.Connectors;
using ThingsEdge.Exchange.Engine.Snapshot;

namespace ThingsEdge.Exchange.Interfaces.Impls;

/// <summary>
/// 标记数据读写接口实现类。
/// </summary>
internal sealed class TagReaderWriterImpl(ITagDataSnapshot tagDataSnapshot,
    IAddressFactory deviceFactory,
    IDriverConnectorManager driverConnectorManager) : ITagReaderWriter
{
    public async Task<(bool ok, PayloadData? data, string? err)> ReadAsync(string tagId)
    {
        var (tag, deviceId) = GetTagWithDevice(tagId);
        if (tag == null)
        {
            return (false, default, "指定的标记不在标记配置中。");
        }

        var driver = driverConnectorManager.GetConnector(deviceId!);
        if (driver == null)
        {
            return (false, default, "没有找到指定设备的连接驱动");
        }

        return await driver.ReadAsync(tag).ConfigureAwait(false);
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

    public async Task<(bool ok, List<PayloadData>? data, string? err)> ReadMultiAsync(string[] tagIds, bool mulitple = true)
    {
        if (tagIds.Length == 0)
        {
            throw new ArgumentNullException(nameof(tagIds));
        }

        string? deviceId = null;
        List<Tag> tags = new(tagIds.Length);
        foreach (var tagId in tagIds)
        {
            var (tag, deviceId0) = GetTagWithDevice(tagId);
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

        // 无法判断快照中是否是最新的数据，因此会直接从设备中读取。
        var driver = driverConnectorManager.GetConnector(deviceId!);
        if (driver == null)
        {
            return (false, default, "没有找到指定设备的连接驱动");
        }

        return await driver.ReadMultiAsync(tags, mulitple).ConfigureAwait(false);
    }

    public async Task<(bool ok, string? err)> WriteAsync(string tagId, object data)
    {
        var (tag, deviceId) = GetTagWithDevice(tagId);
        if (tag == null)
        {
            return (false, "指定的标记不在标记配置中。");
        }

        var driver = driverConnectorManager.GetConnector(deviceId!);
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

    /// <summary>
    /// 通过 tagId 找到对应的 deviceId。
    /// </summary>
    private (Tag? tag, string? deviceId) GetTagWithDevice(string tagId)
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
