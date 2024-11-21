namespace ThingsEdge.Communication.Core;

/// <summary>
/// 用于通信的锁的基类
/// </summary>
public class CommunicationLockNone : ICommunicationLock, IDisposable
{
    private bool _disposedValue;

    public virtual OperateResult EnterLock(int timeout)
    {
        return OperateResult.CreateSuccessResult();
    }

    public virtual void LeaveLock()
    {
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
        }
    }

    /// <inheritdoc cref="M:System.IDisposable.Dispose" />
    public void Dispose()
    {
        if (!_disposedValue)
        {
            Dispose(disposing: true);
            _disposedValue = true;
        }
    }
}
