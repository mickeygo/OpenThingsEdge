namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// 请求后响应结果。
/// </summary>
public sealed class ResponseResult : AbstractResult<ResponseMessage>
{
    public static ResponseResult FromOk(ResponseMessage data)
    {
        return new ResponseResult() { Data = data };
    }

    public static ResponseResult FromOk(int code, ResponseMessage data)
    {
        return new ResponseResult() { Code = code, Data = data };
    }

    public static ResponseResult FromError(int code, string errMessage)
    {
        return new ResponseResult() { Code = code, ErrorMessage = errMessage };
    }

    public static ResponseResult FromError(ErrorCode code, string errMessage)
    {
        return new ResponseResult() { Code = (int)code, ErrorMessage = errMessage };
    }
}
