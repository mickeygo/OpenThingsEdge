namespace ThingsEdge.Application.Configuration;

/// <summary>
/// 标签名称定义。
/// </summary>
public sealed class TagDefine
{
    /// <summary>
    /// 报警。
    /// </summary>
    public string PLC_Alarm { get; init; } = "PLC_Alarm";

    /// <summary>
    /// 设备运行状态。
    /// </summary>
    public string PLC_Equipment_State { get; init; } = "PLC_Equipment_State";

    /// <summary>
    /// 设备运行模式。
    /// </summary>
    public string PLC_Equipment_Mode { get; init; } = "PLC_Equipment_Mode";

    /// <summary>
    /// 进站信号。
    /// </summary>
    public string PLC_Entry_Sign { get; init; } = "PLC_Entry_Sign";

    /// <summary>
    /// 进站的SN。
    /// </summary>
    public string PLC_Entry_SN { get; init; } = "PLC_Entry_SN";

    /// <summary>
    /// 出站信号。
    /// </summary>
    public string PLC_Archive_Sign { get; init; } = "PLC_Archive_Sign";

    /// <summary>
    /// 出站的SN。
    /// </summary>
    public string PLC_Archive_SN { get; init; } = "PLC_Archive_SN";

    /// <summary>
    /// 出站的SN状态（1=>OK/2=>NG）。
    /// </summary>
    public string PLC_Archive_Pass { get; init; } = "PLC_Archive_Pass";

    /// <summary>
    /// 扫关键物料条码信号。
    /// </summary>
    public string PLC_Barcode_Sign { get; init; } = "PLC_Barcode_Sign";
}
