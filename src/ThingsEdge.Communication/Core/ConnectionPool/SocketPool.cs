using System.Collections.Concurrent;
using System.Net.Sockets;

namespace ThingsEdge.Communication.Core.ConnectionPool;

/// <summary>
/// Socket 连接池。
/// </summary>
/// <param name="host">主机</param>
/// <param name="port">端口</param>
/// <param name="maxSize">连接池允许的最大数量。</param>
internal sealed class SocketPool(string host, int port, int maxSize) : IDisposable
{
    private readonly ConcurrentBag<SocketWrapper> _pool = [];
    private readonly SemaphoreSlim _semaphore = new(maxSize, maxSize); // 设置初始信号和并发访问数量都为 maxSize
    private int _currentSize;

    private readonly object _lock = new();

    /// <summary>
    /// 获取当前连接池连成功创建接数的数量。
    /// </summary>
    public int CurrentSize => _currentSize;

    /// <summary>
    /// 获取或设置连接过期时间，超过指定时间未被使用会进行清理，默认 60s。
    /// </summary>
    public TimeSpan ExpiredTime { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// 获取或设置连接超时时长，默认为 5s。
    /// </summary>
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 获取或设置 Socket 保活时长，大于 0 时有效，单位 ms。
    /// </summary>
    public int KeepAliveTime { get; set; }

    /// <summary>
    /// 获取可用的连接。
    /// </summary>
    /// <remarks>若创建的连接已到达连接池最大容量，且连接池为空，会进行等待。</remarks>
    /// <returns></returns>
    /// <exception cref="OperationCanceledException"></exception>
    /// <exception cref="SocketException"></exception>
    public async Task<SocketWrapper> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        // 等待可用的许可
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // 尝试取一个连接，若连接已断开，则释放此连接
            // ConcurrentBag 操作是无序的
            while (_pool.TryTake(out var wrapper))
            {
                if (!wrapper.Available || wrapper.IsExpired(ExpiredTime))
                {
                    wrapper.Dispose();
                    Interlocked.Decrement(ref _currentSize);
                    continue;
                }
                wrapper.InUse = true;
                return wrapper;
            }

            var socket = await CreateSocketAsync().ConfigureAwait(false);
            Interlocked.Increment(ref _currentSize);

            return new SocketWrapper(socket) { InUse = true };
        }
        catch
        {
            _semaphore.Release(); // 未得到可用的连接时，释放许可
            throw;
        }
    }

    /// <summary>
    /// 归还连接。
    /// </summary>
    /// <param name="wrapper">要归还的连接。</param>
    /// <param name="clearPoolWhenUnavailable">当连接不可用时是否清空连接池中的其他连接</param>
    public void ReleaseConnection(SocketWrapper wrapper, bool clearPoolWhenUnavailable = true)
    {
        // 不可用的连接直接释放
        if (!wrapper.Available)
        {
            wrapper.Dispose();

            // 连接不可用时，一般是远程服务访问不了，此处将池中其他连接一并释放掉。如果池中存在连接且被取出使用，
            if (clearPoolWhenUnavailable)
            {
                Clear();
            }
        }
        else
        {
            wrapper.InUse = false;
            wrapper.LastUsedTime = DateTime.Now;

            // 将连接放回池中
            _pool.Add(wrapper);
        }

        // 释放一个许可
        _semaphore.Release();
    }

    /// <summary>
    /// 清理连接池，清除不用用或已过期的连接。
    /// </summary>
    public void Cleanup()
    {
        while (_pool.TryTake(out var wrapper))
        {
            if (!wrapper.Available || wrapper.IsExpired(ExpiredTime))
            {
                wrapper.Dispose();
                _semaphore.Release(); // 回收时释放许可
            }
            else
            {
                // 如果可用且没有过期，重新加入池中
                _pool.Add(wrapper);
            }
        }
    }

    /// <summary>
    /// 清空连接池，并释放所有连接对象。
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            while (_pool.TryTake(out var wrapper))
            {
                wrapper.Dispose();
            }
        }
    }

    private async Task<Socket> CreateSocketAsync()
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        if (KeepAliveTime > 0)
        {
            socket.SetKeepAlive(KeepAliveTime, KeepAliveTime);
        }

        using CancellationTokenSource cts = new(ConnectTimeout);
        await socket.ConnectAsync(host, port, cts.Token).ConfigureAwait(false);
        return socket;
    }

    public void Dispose()
    {
        Clear();
        _semaphore.Dispose();
    }
}
