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

    /// <summary>
    /// 写入数据。
    /// </summary>
    /// <param name="connector">设备连接器。</param>
    /// <param name="tag">要写入的标记。</param>
    /// <param name="data">要写入的数据。</param>
    /// <returns></returns>
    /// <remarks>要写入的数据必须与标记的数据类型匹配，或是可转换为标记设定的类型。</remarks>
    public static async Task<(bool ok, string err)> WriteAsync(this DriverConnector connector, Tag tag, object data)
    {
        try
        {
            return await NetDataReaderWriterUtil.WriteSingleAsync(connector.Driver, tag, data);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
