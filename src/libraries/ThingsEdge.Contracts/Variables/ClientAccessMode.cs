namespace ThingsEdge.Contracts.Variables;

/// <summary>
/// 客户端访问模式。
/// </summary>
public enum ClientAccessMode
{
    /// <summary>
    /// 只读。
    /// </summary>
    [Display(Name = "只读")]
    Read = 1,

    /// <summary>
    /// 读写。
    /// </summary>
    [Display(Name = "读写")]
    ReadAndWrite = 2,
}
