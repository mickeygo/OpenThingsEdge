namespace ThingsEdge.Communication.Core;

/// <summary>
/// 用于通信的锁的基类
/// </summary>
public class CommunicationLockNone : ICommunicationLock, IDisposable
{
    private bool disposedValue = false;

    /// <inheritdoc cref="M:HslCommunication.Core.ICommunicationLock.EnterLock(System.Int32)" />
    public virtual OperateResult EnterLock(int timeout)
    {
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc cref="M:HslCommunication.Core.ICommunicationLock.LeaveLock" />
    public virtual void LeaveLock()
    {
    }

    /// <inheritdoc cref="M:System.IDisposable.Dispose" />
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
        }
    }

    /// <inheritdoc cref="M:System.IDisposable.Dispose" />
    public void Dispose()
    {
        if (!disposedValue)
        {
            Dispose(disposing: true);
            disposedValue = true;
        }
    }
}
