namespace ThingsEdge.Providers.Ops.Configuration;

/// <summary>
/// OPS 参数配置。
/// </summary>
public sealed class OpsConfig
{
    /// <summary>
    /// 默认的标记扫描速率, 默认为 100ms。
    /// </summary>
    public int DefaultScanRate { get; init; } = 100;

    /// <summary>
    /// 开关启动后曲线数据扫描速率，默认为 100ms。
    /// </summary>
    public int SwitchScanRate { get; init; } = 100;

    /// <summary>
    /// 是否尝试批量读取，默认为 true。
    /// </summary>
    /// <remarks>
    /// <para>西门子S7驱动支持离散批量读取，可以始终设置为 true；</para>
    /// <para>三菱MC协议驱动目前支持连续批量读取，若非连续地址需要设置为 false；</para>
    /// <para>其他协议驱动目前还不支持批量读取，内部会采取逐一方式进行读取。</para>
    /// </remarks>
    public bool AllowReadMulti { get; init; } = true;

    /// <summary>
    /// 在触发标志位值回写失败时，是否触发回执值，默认为 false。
    /// </summary>
    /// <remarks>触发标志位重置值，可以防止 PLC 值与 TagSe 值一致导致跳过处理逻辑</remarks>
    public bool AckWhenCallbackError { get; init; }

    /// <summary>
    /// 在触发标志位值回写失败时，允许触发回执的最大次数，默认为 3。
    /// </summary>
    public int AckMaxVersion { get; init; } = 3;

    /// <summary>
    /// 曲线数据配置。
    /// </summary>
    public CurveConfig Curve { get; init; } = new();
}
