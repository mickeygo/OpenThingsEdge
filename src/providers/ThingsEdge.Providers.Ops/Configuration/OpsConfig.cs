namespace ThingsEdge.Providers.Ops.Configuration;

/// <summary>
/// OPS 参数配置。
/// </summary>
public sealed class OpsConfig
{
    /// <summary>
    /// 默认的标记扫描速率, 默认为 200ms。
    /// </summary>
    public int DefaultScanRate { get; init; } = 200;

    /// <summary>
    /// 开关启动后数据扫描速率，默认为 100ms。
    /// </summary>
    public int SwitchScanRate { get; init; } = 100;

    /// <summary>
    /// 曲线数据配置。
    /// </summary>
    public CurveConfig Curve { get; init; } = new();
}
