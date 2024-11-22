namespace ThingsEdge.Communication.Core;

/// <summary>
/// 通信对象的异步锁对象。
/// </summary>
internal sealed class CommAsyncLock : ICommAsyncLock
{
    private readonly Nito.AsyncEx.AsyncLock _mutex = new();

    public async Task<IDisposable> LockAsync(CancellationToken cancellationToken = default)
    {
        return new CommAsyncLockWrapper(await _mutex.LockAsync(cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// 异步锁包装对象。
    /// </summary>
    private sealed class CommAsyncLockWrapper(IDisposable lockObject) : IDisposable
    {
        public void Dispose()
        {
            lockObject.Dispose();
        }
    }
}

