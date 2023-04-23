using ThingsEdge.Router.Events;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 日志消息记录事件处理器。
/// </summary>
internal sealed class MessageLoggedHandler : INotificationHandler<MessageLoggedEvent>
{
    public MessageLoggedHandler()
    {
        
    }

    public Task Handle(MessageLoggedEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
