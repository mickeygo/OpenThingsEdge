using ThingsEdge.Common.EventBus;
using ThingsEdge.Contrib.Http.Events;
using ThingsEdge.Contrib.Http.Model;

namespace ThingsEdge.Contrib.Http.Health;

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
        await _publisher.Publish(new DestinationHealthCheckedEvent { HealthState = healthState }, 
            PublishStrategy.AsyncContinueOnException, cancellationToken).ConfigureAwait(false);
    }
}
