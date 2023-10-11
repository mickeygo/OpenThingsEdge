namespace ThingsEdge.Contracts.Variables;

/// <summary>
/// 客户端访问模式。
/// </summary>
public enum ClientAccessMode
{
    /// <summary>
    /// 只读。
    /// </summary>
    [Description("只读")]
    Read = 1,

    /// <summary>
    /// 读写。
    /// </summary>
    [Description("读写")]
    ReadAndWrite = 2,
}
