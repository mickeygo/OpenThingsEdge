namespace ThingsEdge.Contracts;

/// <summary>
/// 读取加载数据接口。
/// </summary>
public interface IDataReader
{
    /// <summary>
    /// 读取数据。
    /// </summary>
    /// <param name="schema">标记 schema。</param>
    /// <param name="tags">要读取数据的标记集合。</param>
    /// <returns></returns>
    Task<List<DataReadResult>> ReadAsync(Schema schema, IEnumerable<string> tags);
}
