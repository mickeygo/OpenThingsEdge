using ThingsEdge.Router.Events;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 日志消息记录事件处理器。
/// </summary>
internal sealed class MessageLoggedHandler : INotificationHandler<MessageLoggedEvent>
{
    public async Task Handle(MessageLoggedEvent notification, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
