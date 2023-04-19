namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// 传输数据上下文对象。
/// </summary>
public sealed class ForwarderContext
{
    private static readonly AsyncLocal<ForwarderContext?> Ctx = new();

    /// <summary>
    /// 当前载体上下文对象。
    /// </summary>
    public static ForwarderContext? Current => Ctx.Value;

    /// <summary>
    /// 请求消息。
    /// </summary>
    public RequestMessage Request { get; }

    /// <summary>
    /// 响应消息。
    /// </summary>
    public ResponseMessage Response { get; }

    private ForwarderContext(RequestMessage request, ResponseMessage response)
    {
        Request = request;
        Response = response;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ForwarderContext Create(RequestMessage request, ResponseMessage response)
    {
        var ctx = new ForwarderContext(request, response);
        Ctx.Value = ctx;
        return ctx;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Reset()
    {
        if (Ctx.Value != null)
        {
            Ctx.Value = null;
        }
    }
}
