using ThingsEdge.Providers.Ops.Exchange.Monitors;

namespace ThingsEdge.Providers.Ops.Events;

/// <summary>
/// 事件循环
/// </summary>
internal sealed class EventLoop : ISingletonDependency
{
    private readonly InternalEventBroker _broker;
    private readonly EventDispatcher _dispatcher;

    public EventLoop(InternalEventBroker broker, EventDispatcher dispatcher)
    {
        _broker = broker;
        _dispatcher = dispatcher;

        // 注册监控器
        MonitorLoop.Register<HeartbeatMonitor>();
        MonitorLoop.Register<NoticeMonitor>();
        MonitorLoop.Register<TriggerMonitor>();
        MonitorLoop.Register<SwitchMonitor>();
    }

    public async Task Loop(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var @event = await _broker.DequeueAsync(cancellationToken).ConfigureAwait(false);
            await _dispatcher.DispatchAsync(@event).ConfigureAwait(false);
        }
    }
}
