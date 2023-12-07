namespace ThingsEdge.Contracts.Variables;

/// <summary>
/// 标记曲线数据用途。
/// </summary>
/// <remarks>此标记用于标明 Tag 有特殊用途，在数据处理时会根据此来区分使用。</remarks>
public enum TagCurveUsage : int
{
    /// <summary>
    /// 无
    /// </summary>
    [Description("无")]
    None = 0,

    /// <summary>
    /// 表示此标记为曲线数据要绑定的 SN。
    /// </summary>
    [Description("曲线 SN")]
    SwitchSN,

    /// <summary>
    /// 表示此标记为曲线数据要绑定的 SN 对应的序号。
    /// </summary>
    [Description("曲线序号")]
    SwitchNo,

    /// <summary>
    /// 表示此标记为曲线数据。
    /// </summary>
    [Description("曲线数据")]
    SwitchCurve,
}
