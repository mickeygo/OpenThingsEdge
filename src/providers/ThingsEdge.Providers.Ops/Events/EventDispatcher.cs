using ThingsEdge.Common.EventBus;

namespace ThingsEdge.Providers.Ops.Events;

/// <summary>
/// 事件派发器，派发的事件必须实现 <see cref="IMonitorEvent"/> 接口。
/// </summary>
internal sealed class EventDispatcher : ISingletonDependency
{
    private readonly IEventPublisher _publisher;

    public EventDispatcher(IEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task DispatchAsync(IMonitorEvent @event)
    {
        // 事件处理都采用异步不阻塞的方式，也无需等待返回结果
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
        else if (@event is ExchangeChangedEvent exchangeChangedEvent)
        {
            await _publisher.Publish(exchangeChangedEvent, PublishStrategy.ParallelNoWait).ConfigureAwait(false);
        }
    }
}
