namespace ThingsEdge.Router.Interfaces;

/// <summary>
/// 标记数据读写接口。
/// </summary>
public interface ITagReaderWriter
{
    /// <summary>
    /// 读取数据。
    /// </summary>
    /// <param name="deviceId">设备Id，每个设备 Id 都是唯一的，且存在唯一的连接。</param>
    /// <param name="tag">要读取的标记集合。</param>
    /// <returns></returns>
    Task<(bool ok, PayloadData? data, string? err)> ReadAsync(string deviceId, Tag tag);

    /// <summary>
    /// 读取数据。
    /// </summary>
    /// <param name="deviceId">设备Id，每个设备 Id 都是唯一的，且存在唯一的连接。</param>
    /// <param name="tags">要读取的标记集合。</param>
    /// <param name="mulitple">是否允许一次性批量读取。部分设备支持批量读取，但要注意可能需要使用连续地址。</param>
    /// <returns></returns>
    Task<(bool ok, List<PayloadData>? data, string? err)> ReadAsync(string deviceId, IEnumerable<Tag> tags, bool mulitple = true);

    /// <summary>
    /// 写入数据。
    /// </summary>
    /// <param name="deviceId">设备Id，每个设备 Id 都是唯一的，且存在唯一的连接。</param>
    /// <param name="tag">要写入的标记。</param>
    /// <param name="data">要写入的标记的值。</param>
    /// <returns></returns>
    Task<(bool ok, string? err)> WriteAsync(string deviceId, Tag tag, object data);
}
