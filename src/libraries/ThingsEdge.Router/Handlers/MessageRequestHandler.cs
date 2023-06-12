using ThingsEdge.Router.Events;
using ThingsEdge.Router.Interfaces;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 请求消息预处理事件处理者。
/// </summary>
internal sealed class MessageRequestHandler : INotificationHandler<MessageRequestEvent>
{
    private readonly IServiceProvider _serviceProvider;

    public MessageRequestHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Handle(MessageRequestEvent notification, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var msgReqApi = scope.ServiceProvider.GetService<IMessageRequestApi>();
        if (msgReqApi is not null)
        {
            await msgReqApi.PostAsync(notification.LastPayload, notification.Message, cancellationToken).ConfigureAwait(false);
        }
    }
}
