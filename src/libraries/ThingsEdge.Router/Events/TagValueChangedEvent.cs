namespace ThingsEdge.Router.Events;

/// <summary>
/// 标记数据更改事件。
/// </summary>
public sealed class TagValueChangedEvent : INotification
{
    /// <summary>
    /// 事件创建时间。
    /// </summary>
    public DateTime EventTime { get; init; } = DateTime.Now;

    /// <summary>
    /// 标记值集合。
    /// </summary>
    [NotNull]
    public IEnumerable<PayloadData>? Values { get; init; }

    public static TagValueChangedEvent Create(PayloadData Value)
    {
        return new TagValueChangedEvent
        {
            Values = new PayloadData[] { Value },
        };
    }

    public static TagValueChangedEvent Create(IEnumerable<PayloadData> Values)
    {
        return new TagValueChangedEvent
        {
            Values = Values,
        };
    }
}
