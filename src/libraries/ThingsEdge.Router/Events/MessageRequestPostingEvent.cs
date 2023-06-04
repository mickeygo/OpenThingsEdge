namespace ThingsEdge.Router.Events;

/// <summary>
/// 消息请求预处理事件。
/// </summary>
public sealed class MessageRequestPostingEvent : INotification, IEvent
{
    /// <summary>
    /// 事件创建时间。
    /// </summary>
    public DateTime EventTime { get; init; } = DateTime.Now;

    /// <summary>
    /// 请求消息
    /// </summary>
    [NotNull]
    public RequestMessage? Message { get; init; }

    /// <summary>
    /// 上一次触发点的值，若是第一次请求，会为 null。
    /// </summary>
    public PayloadData? LastPayload { get; init; }

    public static MessageRequestPostingEvent Create(RequestMessage message, PayloadData? lastPayload = null)
    {
        return new MessageRequestPostingEvent
        {
            Message = message,
            LastPayload = lastPayload,
        };
    }
}
