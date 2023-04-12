namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// HTTP 请求后响应结果。
/// </summary>
public sealed class HttpResult : AbstractResult<ResponseMessage>
{
    public static HttpResult FromOk(ResponseMessage data)
    {
        return new HttpResult() { Data = data };
    }

    public static HttpResult FromOk(int code, ResponseMessage data)
    {
        return new HttpResult() { Code = code, Data = data };
    }

    public static HttpResult FromError(int code, string errMessage)
    {
        return new HttpResult() { Code = code, ErrorMessage = errMessage };
    }

    public static HttpResult FromError(ErrorCode code, string errMessage)
    {
        return new HttpResult() { Code = (int)code, ErrorMessage = errMessage };
    }
}
