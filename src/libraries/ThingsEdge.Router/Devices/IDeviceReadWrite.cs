namespace ThingsEdge.Router.Devices;

/// <summary>
/// 设备数据读写接口。
/// </summary>
public interface IDeviceReadWrite
{
    /// <summary>
    /// 读取数据。
    /// </summary>
    /// <remarks>根据驱动的支持程度选择数据是逐个读取还是连续读取。</remarks>
    /// <param name="deviceId">设备Id。</param>
    /// <param name="tags">要读取的标记集合。</param>
    /// <returns></returns>
    Task<DeviceReadResult> ReadAsync(string deviceId, IEnumerable<Tag> tags);

    /// <summary>
    /// 写入数据。
    /// </summary>
    /// <param name="deviceId">设备Id。</param>
    /// <param name="tag">要写入的标记。</param>
    /// <param name="data">要写入的值。</param>
    /// <returns></returns>
    Task<(bool ok, string? err)> WriteAsync(string deviceId, Tag tag, object data);
}
