using ThingsEdge.Common.EventBus;
using ThingsEdge.Contrib.Http.Events;
using ThingsEdge.Contrib.Http.Model;

namespace ThingsEdge.Contrib.Http.Health;

internal sealed class HealthCheckHandlePolicy(IEventPublisher publisher) : IHealthCheckHandlePolicy
{
    public async Task HandleAsync(DestinationHealthState healthState, CancellationToken cancellationToken)
    {
        // 通知目标服务健康状况。
        await publisher.Publish(new DestinationHealthCheckedEvent(healthState), 
            PublishStrategy.AsyncContinueOnException, cancellationToken).ConfigureAwait(false);
    }
}
