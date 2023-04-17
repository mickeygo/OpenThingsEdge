namespace ThingsEdge.Contracts.Devices;

/// <summary>
/// 标记数据用途。
/// </summary>
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
    SwitchIndex,

    /// <summary>
    /// 表示此标记为曲线数据。
    /// </summary>
    SwitchCurve,
}
