using System.Threading.Channels;
using ThingsEdge.Router.Events;

namespace ThingsEdge.Providers.Ops.Events;

internal sealed class InternalEventBroker : IDisposable, ISingletonDependency
{
    private const int _capacity = 1024;
    private readonly Channel<IEvent> _channel;

    public InternalEventBroker()
    {
        // 多个写入端，单个读取端
        _channel = System.Threading.Channels.Channel.CreateBounded<IEvent>(new BoundedChannelOptions(_capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        });
    }

    public async ValueTask WriteAsync(IEvent item, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(item, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Wait until there is work or the channel is closed.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> WaitToReadAsync()
    {
        // Wait until there is work or the channel is closed.
        return await _channel.Reader.WaitToReadAsync().ConfigureAwait(false);
    }

    public async ValueTask<IEvent> ReadAsync()
    {
        return await _channel.Reader.ReadAsync().ConfigureAwait(false);
    }

    public bool TryRead([MaybeNullWhen(false)] out IEvent item)
    {
        return _channel.Reader.TryRead(out item);
    }

    public void Dispose()
    {
        _channel.Writer.Complete();
    }
}
