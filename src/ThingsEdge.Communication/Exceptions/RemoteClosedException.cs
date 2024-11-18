namespace ThingsEdge.Communication.Exceptions;

/// <summary>
/// 远程对象已关闭的异常信息。
/// </summary>
public sealed class RemoteClosedException : CommunicationException
{
    /// <summary>
    /// 实例化一个默认的对象
    /// </summary>
    public RemoteClosedException()
        : base("Remote Closed Exception")
    {
    }
}
