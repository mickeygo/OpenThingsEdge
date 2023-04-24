using ThingsEdge.Router.Events;
using ThingsEdge.Router.Model;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 标记数据已读取事件处理者。
/// </summary>
internal sealed class TagValueReadHandler : INotificationHandler<TagValueReadEvent>
{
    private readonly TagDataMonitor _tagDataManager;

    public TagValueReadHandler(TagDataMonitor tagDataManager)
    {
        _tagDataManager = tagDataManager;
    }

    public Task Handle(TagValueReadEvent notification, CancellationToken cancellationToken)
    {
        _tagDataManager.Set(notification.Value.TagId, notification.Value);
        return Task.CompletedTask;
    }
}
