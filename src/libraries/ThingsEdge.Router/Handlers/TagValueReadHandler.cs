using ThingsEdge.Router.Events;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 标记数据已读取事件处理者。
/// </summary>
public sealed class TagValueReadHandler : INotificationHandler<TagValueReadEvent>
{
    public Task Handle(TagValueReadEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
