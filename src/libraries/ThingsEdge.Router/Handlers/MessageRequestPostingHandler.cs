using ThingsEdge.Router.Events;
using ThingsEdge.Router.Interfaces;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 请求消息预处理事件处理者。
/// </summary>
internal sealed class MessageRequestPostingHandler : INotificationHandler<MessageRequestPostingEvent>
{
    private readonly IServiceProvider _serviceProvider;

    public MessageRequestPostingHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Handle(MessageRequestPostingEvent notification, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var msgReqApi = scope.ServiceProvider.GetService<IMessageRequestPostingApi>();
        if (msgReqApi is not null)
        {
            await msgReqApi.PostAsync(notification.LastPayload, notification.Message, cancellationToken).ConfigureAwait(false);
        }
    }
}
