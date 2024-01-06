namespace ThingsEdge.Router.Events;

/// <summary>
/// 曲线文件传送通知事件，其中 <see cref="TagFlag.Switch"/> 会发布此事件
/// </summary>
public sealed class CurveFilePostedEvent : INotification
{
    /// <summary>
    /// 事件创建时间。
    /// </summary>
    public DateTime EventTime { get; init; } = DateTime.Now;

    [NotNull]
    public string? ChannelName { get; init; }

    [NotNull]
    public string? DeviceName { get; set; }

    [NotNull]
    public string? GroupName { get; init; }

    [NotNull]
    public string? SN { get; init; }

    /// <summary>
    /// 曲线所属的编号。
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
