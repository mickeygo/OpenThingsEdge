namespace ThingsEdge.Communication.Core;

/// <summary>
/// 通信对象的异步锁接口。
/// </summary>
public interface ICommAsyncLock
{
    /// <summary>
    /// 进入锁。
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>返回可释放的锁对象。</returns>
    Task<IDisposable> LockAsync(CancellationToken cancellationToken = default);
}

public static class ICommAsyncLockExtensions
{
    /// <summary>
    /// 进入锁。
    /// </summary>
    /// <param name="asyncLock">异步锁</param>
    /// <param name="timeout">超时时间，单位ms</param>
    /// <returns></returns>
    public static Task<IDisposable> LockAsync(this ICommAsyncLock asyncLock, int timeout)
    {
        CancellationTokenSource cts = new(timeout);
        return asyncLock.LockAsync(cts.Token);
    }
}
