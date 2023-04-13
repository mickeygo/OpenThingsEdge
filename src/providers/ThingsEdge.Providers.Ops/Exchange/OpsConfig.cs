namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// OPS 参数配置。
/// </summary>
public sealed class OpsConfig
{
    /// <summary>
    /// 默认的标记扫描速率。
    /// </summary>
    public int DefaultScanRate { get; set; } = 200;

    /// <summary>
    /// 默认的开关标记扫描速率。
    /// </summary>
    public int DefaultSwitchScanRate { get; set; } = 100;

    /// <summary>
    /// 允许开关处于on状态最长时间（秒）。
    /// </summary>
    public int AllowedSwitchOnlineMaxSeconds { get; set; } = 60;
}
