using System.Threading.Channels;
using ThingsEdge.Router.Events;

namespace ThingsEdge.Providers.Ops.Events;

internal sealed class InternalEventBroker : ISingletonDependency
{
    private const int _capacity = 1024;
    private readonly Channel<IEvent> _queue;

    public InternalEventBroker()
    {
        // 多个写入端，单个读取端
        _queue = System.Threading.Channels.Channel.CreateBounded<IEvent>(new BoundedChannelOptions(_capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        });
    }

    public async ValueTask QueueAsync(IEvent item, CancellationToken cancellationToken = default)
    {
        await _queue.Writer.WriteAsync(item, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 异步读取，若队列中没有数据项时会异步阻塞当前线程。
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<IEvent> DequeueAsync(CancellationToken cancellationToken)
    {
        // 效果等于 WaitToReadAsync + TryRead
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}
