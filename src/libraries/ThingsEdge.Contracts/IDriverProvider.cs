namespace ThingsEdge.Contracts;

/// <summary>
/// 驱动提供者。
/// </summary>
public interface IDriverProvider
{
    /// <summary>
    /// 是否可写入数据。
    /// </summary>
    bool CanWrite { get; }

    /// <summary>
    /// 读取数据。
    /// </summary>
    /// <param name="schema">标记 schema。</param>
    /// <param name="tags">要读取数据的标记集合。</param>
    /// <returns></returns>
    Task<List<DataReadResult>> ReadAsync(Schema schema, IEnumerable<TagData> tags);

    /// <summary>
    /// 写入数据。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="schema">标记 schema。</param>
    /// <param name="tag">要回写的标记</param>
    /// <param name="value">要回写的值</param>
    /// <returns>true 表示数据写入成功。</returns>
    Task<DataWriteResult> WriteAsync<T>(Schema schema, TagData tag, T value);
}
