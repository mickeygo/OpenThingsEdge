using ThingsEdge.Router.Events;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 日志消息记录事件处理器。
/// </summary>
internal sealed class LoggingMessageHandler : INotificationHandler<LoggingMessageEvent>
{
    public async Task Handle(LoggingMessageEvent notification, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
