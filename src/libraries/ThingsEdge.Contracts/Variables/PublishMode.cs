namespace ThingsEdge.Contracts.Variables;

/// <summary>
/// 数据发布模式。
/// </summary>
public enum PublishMode
{
    /// <summary>
    /// 只有在每次数据有更改后，才会发送。
    /// </summary>
    OnlyDataChanged = 1,

    /// <summary>
    /// 每次扫描则发送。
    /// </summary>
    EveryScan,
}
