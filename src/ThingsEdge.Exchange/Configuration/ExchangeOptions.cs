namespace ThingsEdge.Exchange.Configuration;

/// <summary>
/// Exchange 配置选项。
/// </summary>
public sealed class ExchangeOptions
{
    /// <summary>
    /// 触发标记的触发条件值，值大于 0 才有效，默认为 1。
    /// </summary>
    public short TagTriggerConditionValue { get; set; } = 1;

    /// <summary>
    /// Heartbeat 心跳收到值后，是否要重置值并回写给设备，默认为 true。
    /// </summary>
    public bool HeartbeatShouldAckZero { get; set; } = true;

    /// <summary>
    /// 默认的标记扫描速率，配置中不设定会使用此设置, 默认为 100ms。
    /// </summary>
    public int DefaultScanRate { get; init; } = 100;

    /// <summary>
    /// 是否尝试批量读取，默认为 true。
    /// </summary>
    /// <remarks>
    /// <para>西门子S7驱动支持离散批量读取，可以始终设置为 true；</para>
    /// <para>三菱MC协议驱动目前支持连续批量读取，只适用于连续地址，非连续地址需设置为 false；</para>
    /// <para>其他协议驱动目前还不支持批量读取，内部会采取逐一方式进行读取。</para>
    /// </remarks>
    public bool AllowReadMulti { get; init; } = true;

    /// <summary>
    /// 针对于 S7 等协议，PLC 一起读取运行的最多 PDU 长度（byte数量），为 0 时会使用默认长度。  
    /// </summary>
    public int PDUSize { get; set; }

    /// <summary>
    /// 在触发标志位值回写失败时，允许触发回执的最大次数，当大于 0 是有效，默认为 3。
    /// </summary>
    /// <remarks>
    /// 触发标志位重置值，可以防止 PLC 值与 Tag 缓存值一致导致跳过处理逻辑；若设置后，后台结束数据需要做幂等处理。
    /// </remarks>
    public int AckRetryMaxCount { get; init; } = 3;

    /// <summary>
    /// 针对于S7协议，1500 系列一起读取运行的最多 PDU 长度（byte数量），为 0 时会使用默认长度。  
    /// </summary>
    public int Siemens_PDUSizeS1500 { get; set; }

    /// <summary>
    /// 针对于S7协议，1200 系列一起读取运行的最多 PDU 长度（byte数量），为 0 时会使用默认长度。  
    /// </summary>
    public int Siemens_PDUSizeS1200 { get; set; }

    /// <summary>
    /// 针对于S7协议，300 系列一起读取运行的最多 PDU 长度（byte数量），为 0 时会使用默认长度。  
    /// </summary>
    public int Siemens_PDUSizeS300 { get; set; }
}
