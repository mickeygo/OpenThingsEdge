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
    /// 默认的标记扫描速率，配置中不设定会使用此设置, 默认为 500ms。
    /// </summary>
    public int DefaultScanRate { get; init; } = 500;

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
    /// <para>
    /// SIEMENS TIA 选项，信息系统->对 PLC 进行编程->指令->通信->S7 通信->数据一致性:
    /// </para>
    /// <para>
    ///  1. 对于 S7 通信指令“PUT”/“GET”，在编程和组态过程中必须考虑到一致性数据区域的大小。这是因为在目标设备（服务器）的用户程序中没有可以用于与用户程序进行通信数据同步的通信块。
    ///  2. 对于 S7-300 和 C7-300（除CPU 318-2 DP 之外），在操作系统的循环控制点，在保持数据一致性的情况下将通信数据逐块（一块为 32 字节）复制到用户存储器中。
    ///      对于大型数据区域，无法确保数据的一致性。如果要求达到规定的数据一致性，则用户程序中的通信数据不应超过 32 个字节（最多为 8 个字节，视具体版本而定）。
    ///  3. 另一方面，在 S7-400 和 S7-1500 中，大小为 462(480-18)字节的块中的通信数据不在循环控制点处理，而是在程序循环期间的固定时间片内完成。
    ///      变量的一致性则由系统保证。因此，使用指令 "PUT"/"GET" 或者在读/写变量（例如，由 OP 或 OS 读/写）时可以在保持一致性的情况下访问这些通信区。
    /// </para>
    /// </summary>
    public int PDUSize { get; set; }

    /// <summary>
    /// 在触发标志位值回写失败时，允许触发回执的最大次数，当大于 0 是有效，默认为 3。
    /// </summary>
    /// <remarks>
    /// 触发标志位重置值，可以防止 PLC 值与 Tag 缓存值一致导致跳过处理逻辑；若设置后，后台结束数据需要做幂等处理。
    /// </remarks>
    public int AckRetryMaxCount { get; init; } = 3;
}
