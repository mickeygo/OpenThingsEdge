namespace ThingsEdge.Router.Transport.MQTT;

/// <summary>
/// 通知消息。
/// 使用 <see cref="INotificationHandler{TNotification}"/> 可以订阅该消息。
/// </summary>
public sealed class NotificationMessage : INotification
{
    /// <summary>
    /// 消息内容。
    /// </summary>
    public RequestMessage Message { get; }

    public NotificationMessage(RequestMessage message)
    {
        Message = message;
    }
}
