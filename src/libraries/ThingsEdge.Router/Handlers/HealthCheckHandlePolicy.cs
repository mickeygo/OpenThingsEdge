using ThingsEdge.Common.EventBus;

namespace ThingsEdge.Router.Handlers;

internal sealed class HealthCheckHandlePolicy : IHealthCheckHandlePolicy
{
    private readonly IEventPublisher _publisher;

    public HealthCheckHandlePolicy(IEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task HandleAsync(DestinationHealthState healthState, CancellationToken cancellationToken)
    {
        // 通知目标服务健康状况。
        await _publisher.Publish(new DestinationHealthEvent { HealthState = healthState }, cancellationToken).ConfigureAwait(false);
    }
}
