namespace ThingsEdge.Exchange.Contracts.Variables;

/// <summary>
/// 标记的值用途。
/// </summary>
public enum TagValueUsage
{
    /// <summary>
    /// 编号
    /// </summary>
    [Description("编号")]
    No = 1,

    /// <summary>
    /// 值
    /// </summary>
    [Description("数值")]
    Numerical,

    /// <summary>
    /// 结果，如 true/false。
    /// </summary>
    [Description("判定结果")]
    Result,
}
