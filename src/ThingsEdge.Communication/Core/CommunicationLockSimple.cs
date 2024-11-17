using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Core;

/// <summary>
/// 用于通信的一个简单的基元混合锁
/// </summary>
public class CommunicationLockSimple : CommunicationLockNone
{
    private int m_waiters = 0;

    private readonly AutoResetEvent m_waiterLock = new AutoResetEvent(initialState: false);

    /// <summary>
    /// 获取当前锁是否在等待当中<br />
    /// Gets whether the current lock is waiting
    /// </summary>
    public bool IsWaitting => m_waiters != 0;

    /// <inheritdoc />
    public override OperateResult EnterLock(int timeout)
    {
        try
        {
            if (Interlocked.Increment(ref m_waiters) == 1)
            {
                return OperateResult.CreateSuccessResult();
            }
            return m_waiterLock.WaitOne(timeout) ? OperateResult.CreateSuccessResult() : new OperateResult($"Enter lock failed, timeout: {timeout}");
        }
        catch (Exception ex)
        {
            return new OperateResult("Enter lock failed, message: " + ex.Message);
        }
    }

    /// <inheritdoc />
    public override void LeaveLock()
    {
        if (Interlocked.Decrement(ref m_waiters) != 0)
        {
            var flag = m_waiterLock.Set();
        }
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
        }
        m_waiterLock.Close();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "CommunicationLockSimple[" + (IsWaitting ? "Locking" : "Unlock") + "]";
    }
}
