namespace ThingsEdge.Contracts;

/// <summary>
/// 数据读取
/// </summary>
public interface IDataReader
{
    Task<T> ReadAsync<T>(string tag);
}
