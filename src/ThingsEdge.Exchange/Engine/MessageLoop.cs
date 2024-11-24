using ThingsEdge.Exchange.Engine.Handler;
using ThingsEdge.Exchange.Engine.Messages;
using ThingsEdge.Exchange.Infrastructure.Brokers;

namespace ThingsEdge.Exchange.Engine;

/// <summary>
/// 消息轮询器。
/// </summary>
/// <param name="serviceProvider"></param>
internal sealed class MessageLoop(IServiceProvider serviceProvider) : IMessageLoop
{
    private readonly Dictionary<Type, Type> _handlers = new()
    {
        { typeof(IHeartbeatMessageHandler), typeof(IMessageBroker<HeartbeatMessage>) },
    };

    public Task LoopAsync(CancellationToken cancellationToken)
    {
        foreach (var (key, value) in _handlers)
        {
            _ = Task.Run(async () =>
            {
                var handler = (IMessageHandler<IMessage>)serviceProvider.GetRequiredService(key);
                var broker = (IMessageBroker<IMessage>)serviceProvider.GetRequiredService(value);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var message = await broker.PullAsync(cancellationToken).ConfigureAwait(false);
                    await handler.HandleAsync(message, cancellationToken).ConfigureAwait(false);
                }
            }, default);
        }

        return Task.CompletedTask;
    }
}
