namespace ThingsEdge.Application.Forwarder;

/// <summary>
/// Tag 定义。
/// </summary>
internal sealed class TagDefineConstants
{
    /// <summary>
    /// 报警。
    /// </summary>
    public const string Alarm = "PLC_Alarm";

    /// <summary>
    /// 设备运行状态。
    /// </summary>
    public const string EquipmentState = "PLC_Equipment_State";

    /// <summary>
    /// 设备运行模式。
    /// </summary>
    public const string EquipmentMode = "PLC_Equipment_Mode";

    /// <summary>
    /// 进站信号。
    /// </summary>
    public const string InboundSign = "PLC_Entry_Sign";

    /// <summary>
    /// 出站信号。
    /// </summary>
    public const string OutboundSign = "PLC_Archive_Sign";

    /// <summary>
    /// 扫关键物料条码信号。
    /// </summary>
    public const string ScanBarcodeSign = "PLC_ScanBarcode_Sign";
}
