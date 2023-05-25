namespace ThingsEdge.Contracts.Variables;

/// <summary>
/// 客户端访问模式。
/// </summary>
public enum ClientAccessMode
{
    /// <summary>
    /// 只读。
    /// </summary>
    Read = 1,

    /// <summary>
    /// 读写。
    /// </summary>
    ReadAndWrite = 2,
}
