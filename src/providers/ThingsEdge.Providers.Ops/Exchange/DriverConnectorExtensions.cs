using Ops.Communication.Profinet.Siemens;

namespace ThingsEdge.Providers.Ops.Exchange;

public static class DriverConnectorExtensions
{
    /// <summary>
    /// 驱动连接器读取数据。
    /// </summary>
    /// <param name="connector">设备连接器。</param>
    /// <param name="tag">要读取的数据标记。</param>
    /// <returns></returns>
    public static async Task<(bool ok, PayloadData data, string err)> ReadAsync(this DriverConnector connector, Tag tag)
    {
        return await DriverReadWriteUtil.ReadAsync(connector.Driver, tag).ConfigureAwait(false);
    }

    /// <summary>
    /// 批量读取数据，若驱动不支持批量读取，会依次逐个读取标记值。
    /// </summary>
    /// <remarks>读取值中任何一个出错，会中断读取并返回错误。</remarks>
    /// <param name="connector"></param>
    /// <param name="tags">批量读取的标记集合。</param>
    /// <returns></returns>
    public static async Task<(bool ok, List<PayloadData>? data, string? err)> ReadMultiAsync(this DriverConnector connector, IEnumerable<Tag> tags)
    {
        if (connector.Driver is SiemensS7Net siemensS7Net)
        {
            return await siemensS7Net.ReadMultiAsync(tags).ConfigureAwait(false);
        }

        // 没有实现批量读取的驱动，逐一读取。
        List<PayloadData> tagPayloads = new(tags.Count());
        foreach (var tag in tags)
        {
            var (ok, data, err) = await connector.ReadAsync(tag).ConfigureAwait(false);
            if (!ok)
            {
                return (false, default, err);
            }

            tagPayloads.Add(data);
        }

        return (true, tagPayloads, default);
    }

    /// <summary>
    /// 驱动连接器写入数据。
    /// 注：写入数据前可进行数据根式化。
    /// </summary>
    /// <param name="connector">设备连接器。</param>
    /// <param name="tag">要写入的标记。</param>
    /// <param name="data">要写入的数据。</param>
    /// <param name="format">是否写入前格式化数据。</param>
    /// <returns></returns>
    /// <remarks>要写入的数据必须与标记的数据类型匹配，或是可转换为标记设定的类型。</remarks>
    public static async Task<(bool ok, string? err)> WriteAsync(this DriverConnector connector, Tag tag, object data, bool format = true)
    {
        try
        {
            object? data2 = data;
            if (format)
            {
                (var ok1, data2, string? err1) = TagFormater.Format(tag, data);
                if (!ok1)
                {
                    return (false, err1);
                }
            }

            return await DriverReadWriteUtil.WriteAsync(connector.Driver, tag, data2!).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
