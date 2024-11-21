namespace ThingsEdge.Communication.Core;

/// <summary>
/// 一个简单的混合线程同步锁，采用了基元用户加基元内核同步构造实现。
/// </summary>
/// <remarks>
/// 当前的锁适用于，竞争频率比较低，锁部分的代码运行时间比较久的情况，当前的简单混合锁可以达到最大性能。
/// </remarks>
public sealed class SimpleHybirdLock : IDisposable
{
    private bool _disposedValue;

    /// <summary>
    /// 基元用户模式构造同步锁
    /// </summary>
    private int _waiters;

    private int _lock_tick;

    private DateTime _enterlock_time = DateTime.Now;

    /// <summary>
    /// 基元内核模式构造同步锁
    /// </summary>
    private readonly Lazy<AutoResetEvent> _waiterLock = new(() => new AutoResetEvent(initialState: false));

    private static long s_simpleHybirdLockCount;

    private static long s_simpleHybirdLockWaitCount;

    /// <summary>
    /// 获取当前锁是否在等待当中。
    /// </summary>
    public bool IsWaitting => _waiters != 0;

    /// <summary>
    /// 获取当前进入等待锁的数量。
    /// </summary>
    public int LockingTick => _lock_tick;

    /// <summary>
    /// 获取当前组件里正总的所有进入锁的信息。
    /// </summary>
    public static long SimpleHybirdLockCount => s_simpleHybirdLockCount;

    /// <summary>
    /// 当前组件里正在等待的锁的统计信息，此时已经发生了竞争了。
    /// </summary>
    public static long SimpleHybirdLockWaitCount => s_simpleHybirdLockWaitCount;

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
            }
            _waiterLock.Value.Close();
            _disposedValue = true;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
    }

    /// <summary>
    /// 获取锁，可以指定获取锁的超时时间，如果指定的时间没有获取锁，则返回<c>False</c>，反之，返回<c>True</c>。
    /// </summary>
    /// <returns>是否正确的获得锁</returns>
    public bool Enter()
    {
        Interlocked.Increment(ref s_simpleHybirdLockCount);
        if (Interlocked.Increment(ref _waiters) == 1)
        {
            _enterlock_time = DateTime.Now;
            return true;
        }
        Interlocked.Increment(ref s_simpleHybirdLockWaitCount);
        Interlocked.Increment(ref _lock_tick);
        var flag = _waiterLock.Value.WaitOne();
        if (!flag)
        {
            Interlocked.Decrement(ref s_simpleHybirdLockCount);
            Interlocked.Decrement(ref s_simpleHybirdLockWaitCount);
            Interlocked.Decrement(ref _lock_tick);
        }
        else
        {
            _enterlock_time = DateTime.Now;
        }
        return flag;
    }

    /// <summary>
    /// 离开锁
    /// </summary>
    /// <returns>如果该操作成功，返回<c>True</c>，反之，返回<c>False</c></returns>
    public bool Leave()
    {
        Interlocked.Decrement(ref s_simpleHybirdLockCount);
        if (Interlocked.Decrement(ref _waiters) == 0)
        {
            return true;
        }
        var flag = _waiterLock.Value.Set();
        Interlocked.Decrement(ref s_simpleHybirdLockWaitCount);
        Interlocked.Decrement(ref _lock_tick);
        return flag;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (_lock_tick > 0)
        {
            return $"SimpleHybirdLock[WaitOne-{DateTime.Now - _enterlock_time}]";
        }
        if (_waiters != 0)
        {
            return $"SimpleHybirdLock[OneLock-{DateTime.Now - _enterlock_time}]";
        }
        return "SimpleHybirdLock[NoneLock]";
    }
}
