using System.Threading.Channels;

namespace ThingsEdge.Exchange.Infrastructure.Brokers;

/// <summary>
/// 消息队列服务。
/// </summary>
internal sealed class MessageQueueBroker<TMessage> where TMessage : IMessage
{
    private readonly Channel<TMessage> _queue;

    /// <summary>
    /// 初始化一个新的对象。
    /// </summary>
    /// <param name="capacity">容量</param>
    /// <param name="fullMode">当队列满后数据再写入的模式</param>
    /// <param name="singleReader">是否只允许单一源读取，默认 true</param>
    /// <param name="singleWriter">是否只允许单一源写入，默认 false</param>
    public MessageQueueBroker(int capacity, BoundedChannelFullMode fullMode = BoundedChannelFullMode.Wait, bool singleReader = true, bool singleWriter = false)
    {
        // 多个写入端，单个读取端
        _queue = Channel.CreateBounded<TMessage>(new BoundedChannelOptions(capacity)
        {
            FullMode = fullMode,
            SingleReader = singleReader,
            SingleWriter = singleWriter,
        });
    }

    /// <summary>
    /// 将消息写入队列。
    /// </summary>
    /// <param name="message">加入队列的消息项</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask QueueAsync(TMessage message, CancellationToken cancellationToken = default)
    {
        await _queue.Writer.WriteAsync(message, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 异步读取，若队列中没有数据项时会异步阻塞当前线程。
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<TMessage> DequeueAsync(CancellationToken cancellationToken)
    {
        // 效果等于 WaitToReadAsync + TryRead
        return await _queue.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
    }
}
