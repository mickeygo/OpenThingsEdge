using System.Threading.Channels;

namespace ThingsEdge.Exchange.Events;

/// <summary>
/// 事件 Broker。
/// </summary>
internal sealed class InternalEventBroker
{
    private const int Capacity = 1024;
    private readonly Channel<IMonitorEvent> _queue;

    public InternalEventBroker()
    {
        // 多个写入端，单个读取端
        _queue = Channel.CreateBounded<IMonitorEvent>(new BoundedChannelOptions(Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        });
    }

    /// <summary>
    /// 将事件写入队列。
    /// </summary>
    /// <param name="item">加入队列的事件项</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask QueueAsync(IMonitorEvent item, CancellationToken cancellationToken = default)
    {
        await _queue.Writer.WriteAsync(item, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 异步读取，若队列中没有数据项时会异步阻塞当前线程。
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<IMonitorEvent> DequeueAsync(CancellationToken cancellationToken)
    {
        // 效果等于 WaitToReadAsync + TryRead
        return await _queue.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
    }
}
