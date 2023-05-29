using ThingsEdge.Router.Events;
using ThingsEdge.Router.Model;
using ThingsEdge.Router.Pipe;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 日志消息记录事件处理器。
/// </summary>
internal sealed class LoggingMessageHandler : INotificationHandler<LoggingMessageEvent>
{
    public async Task Handle(LoggingMessageEvent notification, CancellationToken cancellationToken)
    {
        await ChannelFactory.LoggingChannel.TryWriteAsync(new LoggingMessage
        {
            LoggedTime = notification.EventTime,
            Level = notification.Level,
            Message = notification.Message,
        }, cancellationToken).ConfigureAwait(false);
    }
}
