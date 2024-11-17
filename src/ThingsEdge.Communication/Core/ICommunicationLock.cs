using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Core;

/// <summary>
/// 用于通信的锁的接口对象，定义了进入锁和离开锁的方法接口信息<br />
/// An interface object for a lock used for communication that defines interface information for the methods used to enter and exit the lock
/// </summary>
public interface ICommunicationLock : IDisposable
{
    /// <summary>
    /// 进入锁操作，指定超时时间，单位毫秒，并返回是否成功获得锁的标记。<br />
    /// Enters the lock operation, specifies the timeout period in milliseconds, and returns a flag as to whether the lock was successfully obtained.
    /// </summary>
    /// <param name="timeout">超时时间</param>
    /// <returns>是否成功的获得锁</returns>
    OperateResult EnterLock(int timeout);

    /// <summary>
    /// 离开锁操作<br />
    /// Lockaway operation
    /// </summary>
    void LeaveLock();
}
