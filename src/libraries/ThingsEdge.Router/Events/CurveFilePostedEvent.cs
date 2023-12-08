namespace ThingsEdge.Router.Events;

/// <summary>
/// 曲线文件事件
/// </summary>
public sealed class CurveFilePostedEvent : INotification, IEvent
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

    public string? SN { get; init; }

    /// <summary>
    /// 编号，没有则为 0。
    /// </summary>
    public string? No { get; init; }

    /// <summary>
    /// 本地文件路径
    /// </summary>
    public string? FilePath { get; init; }
}
