namespace ThingsEdge.Contracts.Variables;

/// <summary>
/// 标记数据用途。
/// </summary>
/// <remarks>此标记用于标明 Tag 有特殊用途，在数据处理时会根据此来区分使用。</remarks>
public enum TagUsage : int
{
    /// <summary>
    /// 主数据，默认。
    /// </summary>
    Master = 1,

    /// <summary>
    /// 工艺参数数据（过程数据）。
    /// </summary>
    Technology,

    /// <summary>
    /// 表示此标记为曲线数据要绑定的SN。
    /// </summary>
    SwitchSN,

    /// <summary>
    /// 表示此标记为曲线数据要绑定的SN对应的序号。
    /// </summary>
    SwitchNo,

    /// <summary>
    /// 表示此标记为曲线数据。
    /// </summary>
    SwitchCurve,
}
