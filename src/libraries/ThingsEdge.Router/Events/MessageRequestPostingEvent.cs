namespace ThingsEdge.Router.Events;

/// <summary>
/// 消息请求预处理事件。
/// </summary>
public sealed class MessageRequestPostingEvent : INotification
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

    public static MessageRequestPostingEvent Create(RequestMessage message)
    {
        return new MessageRequestPostingEvent
        {
            Message = message,
        };
    }
}
