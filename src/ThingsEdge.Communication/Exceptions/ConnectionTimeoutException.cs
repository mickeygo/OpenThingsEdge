namespace ThingsEdge.Communication.Exceptions;

/// <summary>
/// 远程连接超时异常。
/// </summary>
public sealed class ConnectionTimeoutException : CommunicationException
{
    public ConnectionTimeoutException() : base("远程连接超时")
    {
    }
}
