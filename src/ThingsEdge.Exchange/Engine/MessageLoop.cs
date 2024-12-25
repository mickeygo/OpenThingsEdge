using ThingsEdge.Exchange.Engine.Handlers;
using ThingsEdge.Exchange.Engine.Messages;
using ThingsEdge.Exchange.Infrastructure.Brokers;

namespace ThingsEdge.Exchange.Engine;

/// <summary>
/// 消息轮询器。
/// </summary>
/// <param name="serviceProvider"></param>
/// <param name="logger">消息日志</param>
internal sealed class MessageLoop(IServiceProvider serviceProvider, ILogger<MessageLoop> logger) : IMessageLoop
{
    public async Task LoopAsync(CancellationToken cancellationToken)
    {
        await LoopHeartbeatAsync(cancellationToken).ConfigureAwait(false);
        await LoopNoticeAsync(cancellationToken).ConfigureAwait(false);
        await LoopTriggerAsync(cancellationToken).ConfigureAwait(false);
        await LoopSwitchAsync(cancellationToken).ConfigureAwait(false);
    }

    private Task LoopHeartbeatAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var broker = serviceProvider.GetRequiredService<IMessageBroker<HeartbeatMessage>>();
            var handler = serviceProvider.GetRequiredService<IHeartbeatMessageHandler>();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var message = await broker.PullAsync(cancellationToken).ConfigureAwait(false);
                    await handler.HandleAsync(message, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError("[MessageLoop-Heartbeat] 轮询接收处理消息异常，异常消息：{Error}", ex.Message);
                }
            }
        }, default);

        return Task.CompletedTask;
    }

    private Task LoopNoticeAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var broker = serviceProvider.GetRequiredService<IMessageBroker<NoticeMessage>>();
            var handler = serviceProvider.GetRequiredService<INoticeMessageHandler>();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var message = await broker.PullAsync(cancellationToken).ConfigureAwait(false);
                    await handler.HandleAsync(message, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError("[MessageLoop-Notice] 轮询接收处理消息异常，异常消息：{Error}", ex.Message);
                }
            }
        }, default);

        return Task.CompletedTask;
    }

    private Task LoopTriggerAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var broker = serviceProvider.GetRequiredService<IMessageBroker<TriggerMessage>>();
            var handler = serviceProvider.GetRequiredService<ITriggerMessageHandler>();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var message = await broker.PullAsync(cancellationToken).ConfigureAwait(false);
                    await handler.HandleAsync(message, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError("[MessageLoop-Trigger] 轮询接收处理消息异常，异常消息：{Error}", ex.Message);
                }
            }
        }, default);

        return Task.CompletedTask;
    }

    private Task LoopSwitchAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var broker = serviceProvider.GetRequiredService<IMessageBroker<SwitchMessage>>();
            var handler = serviceProvider.GetRequiredService<ISwitchMessageHandler>();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var message = await broker.PullAsync(cancellationToken).ConfigureAwait(false);
                    await handler.HandleAsync(message, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError("[MessageLoop-Switch] 轮询接收处理消息异常，异常消息：{Error}", ex.Message);
                }
            }
        }, default);

        return Task.CompletedTask;
    }
}
