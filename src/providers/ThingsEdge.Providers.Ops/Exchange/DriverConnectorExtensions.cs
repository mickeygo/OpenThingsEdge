namespace ThingsEdge.Providers.Ops.Exchange;

public static class DriverConnectorExtensions
{
    /// <summary>
    /// 读取数据。
    /// </summary>
    /// <param name="connector">设备连接器。</param>
    /// <param name="tag">要读取的数据标记。</param>
    /// <returns></returns>
    public static async Task<(bool ok, PayloadData data, string err)> ReadAsync(this DriverConnector connector, Tag tag)
    {
        return await NetDataReaderWriterUtil.ReadSingleAsync(connector.Driver, tag);
    }

    /// <summary>
    /// 写入数据。
    /// </summary>
    /// <param name="connector">设备连接器。</param>
    /// <param name="data">要写入的数据。</param>
    /// <returns></returns>
    public static async Task<(bool ok, string err)> WriteAsync(this DriverConnector connector, PayloadData data)
    {
        return await NetDataReaderWriterUtil.WriteSingleAsync(connector.Driver, data);
    }
}
