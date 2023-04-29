using ThingsEdge.Router.Events;
using ThingsEdge.Router.Management;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 标记数据已更改事件处理者。
/// </summary>
internal sealed class TagValueChangedHandler : INotificationHandler<TagValueChangedEvent>
{
    private readonly TagDataFactory _tagDataFactory;

    public TagValueChangedHandler(TagDataFactory tagDataFactory)
    {
        _tagDataFactory = tagDataFactory;
    }

    public Task Handle(TagValueChangedEvent notification, CancellationToken cancellationToken)
    {
        _tagDataFactory.Set(notification.Values);
        return Task.CompletedTask;
    }
}
