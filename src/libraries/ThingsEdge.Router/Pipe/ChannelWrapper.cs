using System.Threading.Channels;

namespace ThingsEdge.Router.Pipe;

/// <summary>
/// Channel 包装类。
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class ChannelWrapper<T>
{
    private readonly Channel<T> _channel;

    public ChannelWrapper(Channel<T> channel)
    {
        _channel = channel;
    }

    /// <summary>
    /// 尝试读取数据，若读取失败，返回 null。
    /// </summary>
    /// <remarks>若Channel中没有消息可读，会循环等待可读的消息。</remarks>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<T?> TryReadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _channel.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// 异步处理，读取数据。
    /// </summary>
    /// <param name="callback">读取数据后的回调函数</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task PollReadAsync(Action<T?> callback, CancellationToken cancellationToken = default)
    {
        CancellationTokenSource _cts = new();

        _ = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested
                && await _channel.Reader.WaitToReadAsync().ConfigureAwait(false))
            {
                if (_channel.Reader.TryRead(out var item))
                {
                    callback(item);
                }
            }
        });

        return Task.CompletedTask;
    }

    /// <summary>
    /// 异步处理，读取数据。
    /// </summary>
    /// <param name="asyncCallback">读取数据后的异步回调函数</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task PollReadAsync(Func<T?, Task> asyncCallback, CancellationToken cancellationToken = default)
    {
        CancellationTokenSource _cts = new();

        _ = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested
                && await _channel.Reader.WaitToReadAsync().ConfigureAwait(false))
            {
                if (_channel.Reader.TryRead(out var item))
                {
                    await asyncCallback(item).ConfigureAwait(false);
                }
            }
        });

        return Task.CompletedTask;
    }

    /// <summary>
    /// 异步处理，读取数据。
    /// </summary>
    /// <param name="callback">读取数据后的回调函数</param>
    /// <returns></returns>
    public Task<IDisposable> PollRead2Async(Action<T?> callback)
    {
        CancellationTokenSource cts = new();
        
        _ = Task.Run(async () =>
        {
            while (!cts.IsCancellationRequested && await _channel.Reader.WaitToReadAsync().ConfigureAwait(false))
            {
                if (_channel.Reader.TryRead(out var item))
                {
                    callback(item);
                }
            }
        });

        IDisposable dispose = new DisposableAction(cts);
        return Task.FromResult(dispose);
    }

    /// <summary>
    /// 异步处理，读取数据。
    /// </summary>
    /// <param name="asyncCallback">读取数据后的异步回调函数</param>
    /// <returns></returns>
    public Task<IDisposable> PollRead2Async(Func<T?, Task> asyncCallback)
    {
        CancellationTokenSource cts = new();

        _ = Task.Run(async () =>
        {
            while (!cts.IsCancellationRequested && await _channel.Reader.WaitToReadAsync().ConfigureAwait(false))
            {
                if (_channel.Reader.TryRead(out var item))
                {
                    await asyncCallback(item).ConfigureAwait(false);
                }
            }
        });

        IDisposable dispose = new DisposableAction(cts);
        return Task.FromResult(dispose);
    }


    /// <summary>
    /// 尝试写入数据。
    /// </summary>
    /// <remarks>若Channel中没有空间可写，会循环等待写入消息。</remarks>
    /// <param name="item">要写入的数据。</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<bool> TryWriteAsync(T item, CancellationToken cancellationToken = default)
    {
        try
        {
            await _channel.Writer.WriteAsync(item, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    internal sealed class DisposableAction : IDisposable
    {
        private readonly CancellationTokenSource _cts = new();

        public DisposableAction(CancellationTokenSource cts)
        {
            _cts = cts;
        }8

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
