namespace ThingsEdge.Application.Forwarder;

internal class TagForwardConstants
{
    /// <summary>
    /// 警报。
    /// </summary>
    public const string Alarm = "PLC_Alarm";

    /// <summary>
    /// 设备状态（空闲、工作、故障、停机）。
    /// </summary>
    public const string State = "PLC_Equipment_State";

    /// <summary>
    /// 进站信号。
    /// </summary>
    public const string InboundSign = "PLC_Inbound_Sign";

    /// <summary>
    /// 出站信号。
    /// </summary>
    public const string OutboundSign = "PLC_Outbound_Sign";

    /// <summary>
    /// 扫码信号。
    /// </summary>
    public const string ScanSign = "PLC_ScanBarcode_Sign";
}
