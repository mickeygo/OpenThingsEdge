using ThingsEdge.Exchange.Infrastructure.Brokers;

namespace ThingsEdge.Exchange.Engine.Handler;

/// <summary>
/// 消息处理器
/// </summary>
internal interface IMessageHandler<TMessage> where TMessage : IMessage
{
    /// <summary>
    /// 处理消息。
    /// </summary>
    /// <param name="message">要处理的消息。</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HandleAsync(TMessage message, CancellationToken cancellationToken);
}
