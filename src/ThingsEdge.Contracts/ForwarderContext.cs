namespace ThingsEdge.Contracts;

/// <summary>
/// 传输数据上下文对象。
/// </summary>
public sealed class ForwarderContext
{
    private readonly static AsyncLocal<ForwarderContext> _context = new();

    /// <summary>
    /// 当前数据传输上下文对象。
    /// </summary>
    public static ForwarderContext? Current => _context.Value;

    public RequestMessage Request { get; }

    public ResponseMessage Response { get; }

    public ForwarderContext(RequestMessage request, ResponseMessage response)
    {
        Request = request;
        Response = response;
    }
    
    public void SetContext(ForwarderContext context)
    {
        _context.Value = context;
    }
}
