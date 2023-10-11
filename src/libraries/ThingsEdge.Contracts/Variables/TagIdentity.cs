namespace ThingsEdge.Contracts.Variables;

/// <summary>
/// Tag 身份标识。
/// </summary>
public enum TagIdentity
{
    /// <summary>
    /// 主数据。
    /// </summary>
    [Description("主数据")]
    Master = 1,

    /// <summary>
    /// 附加数据。
    /// </summary>
    [Description("附加数据")]
    Attach = 2,
}
