namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 开关数据上下文。
/// </summary>
/// <param name="ChannelName">通道名称</param>
/// <param name="DeviceName">设备名称</param>
/// <param name="GroupName">组名</param>
/// <param name="Barcode">条码</param>
/// <param name="No">编号</param>
/// <param name="CurveName">曲线名称</param>
/// <param name="FilePath">本地文件路径</param>
public sealed record SwitchContext(
    string ChannelName,
    string DeviceName,
    string? GroupName,
    string Barcode,
    string? No,
    string CurveName,
    string? FilePath);
