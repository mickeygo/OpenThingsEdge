namespace ThingsEdge.Contracts;

/// <summary>
/// 数据操作。
/// </summary>
public interface IDataCommand
{
    /// <summary>
    /// 是否可写入数据。
    /// </summary>
    bool CanWrite { get; }

    /// <summary>
    /// 从设备中读取数据。
    /// </summary>
    /// <param name="schema">标记 schema。</param>
    /// <param name="tags">要读取数据的标记集合。</param>
    /// <returns></returns>
    Task<List<DataReadResult>> ReadAsync(DeviceSchema schema, IEnumerable<TagData> tags);

    /// <summary>
    /// 向设备写入数据。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="schema">标记 schema。</param>
    /// <param name="tag">要回写的标记</param>
    /// <param name="value">要回写的值</param>
    /// <returns>true 表示数据写入成功。</returns>
    Task<DataWriteResult> WriteAsync<T>(DeviceSchema schema, TagData tag, T value);
}
