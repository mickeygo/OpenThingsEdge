namespace ThingsEdge.Contracts;

/// <summary>
/// 回写数据到设备。
/// </summary>
public interface IDataWriter
{
    /// <summary>
    /// 写入数据。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="schema">标记 schema。</param>
    /// <param name="tag">要回写的标记</param>
    /// <param name="value">要回写的值</param>
    /// <returns>true 表示数据写入成功。</returns>
    Task<bool> WriteAsync<T>(Schema schema, string tag, T value);
}
