namespace ThingsEdge.Communication.Core;

/// <summary>
/// 用于通信的锁的接口对象，定义了进入锁和离开锁的方法接口信息。
/// </summary>
public interface ICommunicationLock : IDisposable
{
    /// <summary>
    /// 进入锁操作，指定超时时间，单位毫秒，并返回是否成功获得锁的标记。
    /// </summary>
    /// <param name="timeout">超时时间</param>
    /// <returns>是否成功的获得锁</returns>
    OperateResult EnterLock(int timeout);

    /// <summary>
    /// 离开锁操作。
    /// </summary>
    void LeaveLock();
}
