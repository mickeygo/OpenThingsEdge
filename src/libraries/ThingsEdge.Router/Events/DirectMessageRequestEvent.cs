namespace ThingsEdge.Router.Events;

/// <summary>
/// 消息请求处理通知事件，其中 <see cref="TagFlag.Notice"/>、<see cref="TagFlag.Trigger"/> 和 <see cref="TagFlag.Switch"/> 会发布此事件。
/// </summary>
public sealed class DirectMessageRequestEvent : INotification
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

    public static DirectMessageRequestEvent Create(RequestMessage message, PayloadData? lastPayload = null)
    {
        return new DirectMessageRequestEvent
        {
            Message = message,
            LastPayload = lastPayload,
        };
    }
}
