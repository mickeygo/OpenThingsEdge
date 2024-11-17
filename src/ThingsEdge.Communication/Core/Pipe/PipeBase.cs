namespace ThingsEdge.Communication.Core.Pipe;

/// <summary>
/// 管道的基础类对象
/// </summary>
public class PipeBase : IDisposable
{
    private SimpleHybirdLock hybirdLock;

    /// <inheritdoc cref="P:HslCommunication.Core.SimpleHybirdLock.LockingTick" />
    public int LockingTick => hybirdLock.LockingTick;

    /// <summary>
    /// 实例化一个默认的对象
    /// </summary>
    public PipeBase()
    {
        hybirdLock = new SimpleHybirdLock();
    }

    /// <inheritdoc cref="M:HslCommunication.Core.SimpleHybirdLock.Enter" />
    public bool PipeLockEnter()
    {
        return hybirdLock.Enter();
    }

    /// <inheritdoc cref="M:HslCommunication.Core.SimpleHybirdLock.Leave" />
    public bool PipeLockLeave()
    {
        return hybirdLock.Leave();
    }

    /// <inheritdoc cref="M:System.IDisposable.Dispose" />
    public virtual void Dispose()
    {
        hybirdLock?.Dispose();
    }
}
