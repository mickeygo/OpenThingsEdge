namespace ThingsEdge.Communication.Exceptions;

/// <summary>
/// 远程对象关闭的异常信息。
/// </summary>
public class RemoteCloseException : Exception
{
    /// <summary>
    /// 实例化一个默认的对象
    /// </summary>
    public RemoteCloseException()
        : base("Remote Closed Exception")
    {
    }
}
