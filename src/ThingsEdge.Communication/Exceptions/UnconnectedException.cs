namespace ThingsEdge.Communication.Exceptions;

/// <summary>
/// 还未连接服务器异常。在未连接服务器情况发生数据会抛出此异常。
/// </summary>
public sealed class UnconnectedException : CommunicationException
{
    public UnconnectedException() : base("还未连接服务器，不能进行数据读写操作")
    {
        
    }
}
