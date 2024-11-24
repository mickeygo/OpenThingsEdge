namespace ThingsEdge.Exchange.Infrastructure.Brokers;

/// <summary>
/// 的消息 Broker，容量为 1，允许多目标读取，当只能单一源写入。
/// </summary>
internal sealed class SingleMessageBroker<TMessage> : IMessageBroker<TMessage> where TMessage : IMessage
{
    private readonly MessageQueueBroker<TMessage> _broker;

    public SingleMessageBroker()
    {
        _broker = new MessageQueueBroker<TMessage>(1);
    }

    public ValueTask PushAsync(TMessage message, CancellationToken cancellationToken)
    {
        return _broker.QueueAsync(message, cancellationToken);
    }

    public ValueTask<TMessage> PullAsync(CancellationToken cancellationToken)
    {
        return _broker.DequeueAsync(cancellationToken);
    }
}
