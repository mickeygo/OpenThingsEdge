namespace ThingsEdge.Contracts;

/// <summary>
/// 传输数据上下文对象。
/// </summary>
public sealed class ForwarderContext
{
    public RequestMessage Request { get; }

    public ResponseMessage Response { get; }

    public ForwarderContext(RequestMessage request, ResponseMessage response)
    {
        Request = request;
        Response = response;
    }
}
