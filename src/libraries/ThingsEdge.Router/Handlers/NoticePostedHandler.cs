using ThingsEdge.Router.Events;
using ThingsEdge.Router.Interfaces;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 通知消息事件处理者。
/// </summary>
internal sealed class NoticePostedHandler : INotificationHandler<NoticePostedEvent>
{
    private readonly IServiceProvider _serviceProvider;

    public NoticePostedHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Handle(NoticePostedEvent notification, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var noticeApi = scope.ServiceProvider.GetService<INoticePostedApi>();
        if (noticeApi is not null)
        {
            await noticeApi.NotifyAsync(notification.Message, notification.LastMasterPayload, cancellationToken).ConfigureAwait(false);
        }
    }
}
