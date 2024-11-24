namespace ThingsEdge.Exchange.Infrastructure.EventBus;

/// <summary>
/// 基于 <see cref="Mediator"/> 的自定义扩展。
/// </summary>
internal sealed class CustomMediator : Mediator
{
    private readonly Func<IEnumerable<NotificationHandlerExecutor>, INotification, CancellationToken, Task> _publish;

    public CustomMediator(IServiceProvider serviceFactory, Func<IEnumerable<NotificationHandlerExecutor>, INotification, CancellationToken, Task> publish) : base(serviceFactory)
        => _publish = publish;

    protected override Task PublishCore(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
        => _publish(handlerExecutors, notification, cancellationToken);
}
