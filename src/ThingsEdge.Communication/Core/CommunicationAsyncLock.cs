namespace ThingsEdge.Communication.Core;

/// <summary>
/// 通信对象的异步锁对象。
/// </summary>
internal sealed class CommunicationAsyncLock : ICommunicationAsyncLock
{
    private readonly Nito.AsyncEx.AsyncLock _mutex = new();

    public async Task<IDisposable> LockAsync(CancellationToken cancellationToken = default)
    {
        return new CommunicationAsyncLockWrapper(await _mutex.LockAsync(cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// 异步锁包装对象。
    /// </summary>
    private sealed class CommunicationAsyncLockWrapper(IDisposable lockObject) : IDisposable
    {
        public void Dispose()
        {
            lockObject.Dispose();
        }
    }
}

