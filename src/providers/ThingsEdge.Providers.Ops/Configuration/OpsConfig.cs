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
    /// 是否尝试批量读取。
    /// </summary>
    /// <remarks>
    /// 注：S7 批量读取地址可以是离散的；MelsecMc 批量读取地址必须是连续的。
    /// </remarks>
    public bool AllowReadMulti { get; init; } = true;

    /// <summary>
    /// 针对于S7协议，一起读取运行的最多 PDU 长度（byte数量），为0时会使用从CPU中读取的 PDU 长度。  
    /// </summary>
    public int AllowMaxPDULength { get; init; } = 462;

    /// <summary>
    /// 曲线数据配置。
    /// </summary>
    public CurveConfig Curve { get; init; } = new();
}
