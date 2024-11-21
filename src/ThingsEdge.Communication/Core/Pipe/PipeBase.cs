namespace ThingsEdge.Communication.Core.Pipe;

/// <summary>
/// 管道的基础类对象。
/// </summary>
public class PipeBase : IDisposable
{
    private SimpleHybirdLock _hybirdLock;

    public int LockingTick => _hybirdLock.LockingTick;

    /// <summary>
    /// 实例化一个默认的对象
    /// </summary>
    public PipeBase()
    {
        _hybirdLock = new SimpleHybirdLock();
    }

    public bool PipeLockEnter()
    {
        return _hybirdLock.Enter();
    }

    public bool PipeLockLeave()
    {
        return _hybirdLock.Leave();
    }

    public virtual void Dispose()
    {
        _hybirdLock?.Dispose();
    }
}
