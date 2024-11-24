namespace ThingsEdge.Exchange.Infrastructure.Brokers;

/// <summary>
/// 消息 Broker。
/// </summary>
/// <typeparam name="TMessage">消息对象</typeparam>
internal interface IMessageBroker<TMessage> where TMessage : IMessage
{
    /// <summary>
    /// 推送消息。
    /// </summary>
    /// <param name="message">要推送的消息。</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask PushAsync(TMessage message, CancellationToken cancellationToken);

    /// <summary>
    /// 拉取消息。
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<TMessage> PullAsync(CancellationToken cancellationToken);
}
