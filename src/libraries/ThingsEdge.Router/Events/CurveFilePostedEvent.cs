namespace ThingsEdge.Router.Events;

/// <summary>
/// 曲线文件传送通知事件，其中 <see cref="TagFlag.Switch"/> 会发布此事件
/// </summary>
public sealed record CurveFilePostedEvent : INotification
{
    [NotNull]
    public string? ChannelName { get; init; }

    [NotNull]
    public string? DeviceName { get; set; }

    [NotNull]
    public string? GroupName { get; init; }

    /// <summary>
    /// 条码。
    /// </summary>
    [NotNull]
    public string? Barcode { get; init; }

    /// <summary>
    /// 编号。
    /// </summary>
    public string? No { get; init; }

    /// <summary>
    /// 曲线名称
    /// </summary>
    [NotNull]
    public string? CurveName { get; set; }

    /// <summary>
    /// 本地文件路径
    /// </summary>
    public string? FilePath { get; init; }
}
