using ThingsEdge.Common.EventBus;
using ThingsEdge.Router.Events;

namespace ThingsEdge.Providers.Ops.Events;

/// <summary>
/// 事件派发器
/// </summary>
internal sealed class EventDispatcher : ISingletonDependency
{
    private readonly IEventPublisher _publisher;

    public EventDispatcher(IEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task DispatchAsync(IEvent @event)
    {
        if (@event is HeartbeatEvent heartbeatEvent)
        {
            await _publisher.Publish(heartbeatEvent, PublishStrategy.ParallelNoWait).ConfigureAwait(false);
        }
        else if (@event is NoticeEvent noticeEvent)
        {
            await _publisher.Publish(noticeEvent, PublishStrategy.ParallelNoWait).ConfigureAwait(false);
        }
        else if (@event is TriggerEvent triggerEvent)
        {
            await _publisher.Publish(triggerEvent, PublishStrategy.ParallelNoWait).ConfigureAwait(false);
        }
        else if (@event is SwitchEvent switchEvent)
        {
            await _publisher.Publish(switchEvent, PublishStrategy.ParallelNoWait).ConfigureAwait(false);
        }
        else if (@event is LoggingMessageEvent loggingMessageEvent)
        {
            await _publisher.Publish(loggingMessageEvent, PublishStrategy.ParallelNoWait).ConfigureAwait(false);
        }
    }
}
