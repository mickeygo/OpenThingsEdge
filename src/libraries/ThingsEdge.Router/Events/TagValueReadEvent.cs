namespace ThingsEdge.Router.Events;

/// <summary>
/// 标记数据已读取事件。
/// </summary>
public sealed class TagValueReadEvent : INotification
{
    /// <summary>
    /// 事件创建时间。
    /// </summary>
    public DateTime EventTime { get; init; } = DateTime.Now;

    /// <summary>
    /// 标记。
    /// </summary>
    [NotNull]
    public Tag? Tag { get; init; }

    /// <summary>
    /// 标记值。
    /// </summary>
    [NotNull]
    public PayloadData? Value { get; init; }
}
