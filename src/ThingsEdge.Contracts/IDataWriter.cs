namespace ThingsEdge.Contracts;

/// <summary>
/// 数据写入。
/// </summary>
public interface IDataWriter
{
    Task WriteAsync<T>(T data);
}
