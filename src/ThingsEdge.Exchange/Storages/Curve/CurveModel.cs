namespace ThingsEdge.Exchange.Storages.Curve;

/// <summary>
/// 曲线存储数据模型
/// </summary>
internal sealed class CurveModel
{
    /// <summary>
    /// 曲线条码
    /// </summary>
    public string? Barcode { get; set; }

    /// <summary>
    /// 曲线所属的编号
    /// </summary>
    public string? No { get; set; }

    /// <summary>
    /// 曲线名称
    /// </summary>
    [NotNull]
    public string? CurveName { get; set; }

    /// <summary>
    /// 通道名称
    /// </summary>
    [NotNull]
    public string? ChannelName { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    [NotNull]
    public string? DeviceName { get; set; }

    /// <summary>
    /// 分组名称
    /// </summary>
    public string? GroupName { get; set; }
}
