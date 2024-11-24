namespace ThingsEdge.Exchange.Contracts.Variables;

/// <summary>
/// 数据发布模式。
/// </summary>
public enum PublishMode
{
    /// <summary>
    /// 只有在每次数据有更改后，才会发送。
    /// </summary>
    [Description("每次数据变化后发送")]
    OnlyDataChanged = 1,

    /// <summary>
    /// 每次扫描则发送。
    /// </summary>
    [Description("每次扫描则发送")]
    EveryScan,
}
