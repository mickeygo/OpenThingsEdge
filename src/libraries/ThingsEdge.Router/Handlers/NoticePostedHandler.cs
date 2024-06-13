using ThingsEdge.Router.Events;
using ThingsEdge.Router.Forwarders;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 通知消息事件处理者。
/// </summary>
internal sealed class NoticePostedHandler(INotificationForwarderWrapper notificationForwarderWrapper) : INotificationHandler<NoticePostedEvent>
{
    public async Task Handle(NoticePostedEvent notification, CancellationToken cancellationToken)
    {
        await notificationForwarderWrapper.PublishAsync(notification.Message, notification.LastMasterPayload, cancellationToken).ConfigureAwait(false);
    }
}
