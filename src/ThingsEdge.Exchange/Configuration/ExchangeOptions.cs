namespace ThingsEdge.Exchange.Configuration;

/// <summary>
/// Exchange 配置选项。
/// </summary>
public sealed class ExchangeOptions
{
    /// <summary>
    /// 默认的标记扫描速率（单位：ms），配置中不设定会使用此设置, 默认为 500ms。
    /// </summary>
    public int DefaultScanRate { get; set; } = 500;

    /// <summary>
    /// 是否尝试批量读取，默认为 true。
    /// </summary>
    /// <remarks>
    /// <para>西门子S7驱动支持离散批量读取，可以始终设置为 true；</para>
    /// <para>三菱MC协议驱动目前支持连续批量读取，只适用于连续地址，非连续地址需设置为 false；</para>
    /// <para>其他协议驱动目前还不支持批量读取，内部会采取逐一方式进行读取。</para>
    /// </remarks>
    public bool AllowReadMultiple { get; set; } = true;

    /// <summary>
    /// Heartbeat 心跳收到值后，是否要重置值并回写给设备，默认为 true。
    /// </summary>
    /// <remarks>部分情况下上位机设置心跳但是不需要回写，可设置为 false。</remarks>
    public bool HeartbeatShouldAckZero { get; set; } = true;

    /// <summary>
    /// 监听心跳数据是否采用高电平值，默认为 true。
    /// </summary>
    /// <remarks>使用高电平值时，在监听值为 True 或 1 时会触发，然后回写给设备低电平值（如 False、0 等），不采用则相反。</remarks>
    public bool HeartbeatListenUseHighLevel { get; set; } = true;

    /// <summary>
    /// 触发标记的触发条件值，值大于 0 才有效，默认为 1。
    /// </summary>
    /// <remarks>当 Tag 为 Trigger 类型时，只有在值为该指定值时才会触发。</remarks>
    public short TriggerConditionValue { get; set; } = 1;

    /// <summary>
    /// 使用其他标记地址来回写返回状态值，默认为 false。
    /// </summary>
    /// <remarks>
    /// 使用另外的标记来存储回写值时，该标记只能位于数据回写集合 CallbackTags 中。
    /// </remarks>
    public bool TriggerStateWriteTagUseOther { get; set; }

    /// <summary>
    /// 回写状态标记的后缀名，在参数 <see cref="TriggerStateWriteTagUseOther"/> 为 true 时有效果，默认为 "Response"。
    /// </summary>
    /// <remarks>
    /// 若状态触发标记为 "PLC_Inbound_Sign"，回写状态标记则为 "PLC_Inbound_Sign_Response"。
    /// </remarks>
    public string? TriggerStateWriteOtherTagSuffix { get; set; } = "Response";

    /// <summary>
    /// 在返回状态值与触发值相等时，写回给设备的状态码，默认为 -1（返回状态值在使用同一地址时有效）。
    /// </summary>
    public short TriggerAckCodeWhenEqual { get; set; } = -1;

    /// <summary>
    /// 针对于 S7 等协议，PLC 一起读取运行的最多 PDU 长度（byte数量），为 0 时会使用实际长度。
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
    /// <remarks>优先使用对应配置文件中的设置，若没设置才会使用此设置。</remarks>
    public int MaxPDUSize { get; set; }

    /// <summary>
    /// Socket 连接池最大数量，默认为 1。
    /// </summary>
    /// <remarks>
    /// 针对于 S7、ModubusTcp 等底层采用 TCP 通信协议的驱动可设置连接线程池最大数量。
    /// 注意：三菱 MC 协议同一端口不允许有多个连接。
    /// </remarks>
    public int SocketPoolSize { get; set; } = 1;

    /// <summary>
    /// 网络连接超时时长（单位：ms），默认 3s。
    /// </summary>
    public int NetworkConnectTimeout { get; set; } = 3_000;

    /// <summary>
    /// Socket 保活时长（单位：ms），只有在大于 0 时才启用，默认 60s。
    /// </summary>
    public int NetworkKeepAliveTime { get; set; } = 60_000;

    /// <summary>
    /// 通知消息发送时是否要带上一次信号点的值，默认为 true。
    /// </summary>
    public bool NoticePublishIncludeLast { get; set; } = true;

    /// <summary>
    /// 开关启动后数据扫码速率（单位：ms），默认为 31ms。
    /// </summary>
    public int SwitchScanRate { get; set; } = 31;

    /// <summary>
    /// 曲线数据配置。
    /// </summary>
    public CurveOptions Curve { get; init; } = new();
}
