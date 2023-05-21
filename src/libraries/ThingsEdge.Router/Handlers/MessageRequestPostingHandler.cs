using ThingsEdge.Router.Events;
using ThingsEdge.Router.Interfaces;
using ThingsEdge.Router.Management;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 请求消息预处理事件处理者。
/// </summary>
internal sealed class MessageRequestPostingHandler : INotificationHandler<MessageRequestPostingEvent>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TagDataContainer _tagDataContainer;

    public MessageRequestPostingHandler(IServiceProvider serviceProvider, TagDataContainer tagDataContainer)
    {
        _serviceProvider = serviceProvider;
        _tagDataContainer = tagDataContainer;
    }

    public async Task Handle(MessageRequestPostingEvent notification, CancellationToken cancellationToken)
    {
        var reqMessage = notification.Message;
        var self = reqMessage.Self();

        // 提取最近一次标记值。
        var oldTagData = _tagDataContainer.Get(self.TagId);

        // 更新标记值。
        _tagDataContainer.Set(reqMessage.Schema, reqMessage.Values);

        using var scope = _serviceProvider.CreateScope();
        var msgReqApi = scope.ServiceProvider.GetService<IMessageRequestPostingApi>();
        if (msgReqApi is not null)
        {
            await msgReqApi.PostAsync(oldTagData?.Data, notification.Message, cancellationToken).ConfigureAwait(false);
        }
    }
}
