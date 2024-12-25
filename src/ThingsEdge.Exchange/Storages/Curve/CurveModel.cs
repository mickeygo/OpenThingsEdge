using ThingsEdge.Exchange.Contracts;

namespace ThingsEdge.Exchange.Storages.Curve;

/// <summary>
/// 曲线存储数据模型
/// </summary>
/// <param name="ChannelName">通道名称</param>
/// <param name="DeviceName">设备名称</param>
/// <param name="GroupName">分组名称</param>
/// <param name="CurveName">曲线名称</param>
/// <param name="Masters">主数据集合，顺序与按配置文件一致。</param>
internal sealed record CurveModel(
    string ChannelName,
    string DeviceName,
    string? GroupName,
    string? CurveName,
    IReadOnlyList<PayloadData> Masters);
