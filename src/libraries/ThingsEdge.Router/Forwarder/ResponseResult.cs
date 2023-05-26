namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// 请求后响应结果。
/// </summary>
public sealed class ResponseResult : AbstractResult<ResponseMessage>
{
    /// <summary>
    /// Source 可为 HTTP、MQTT、或 Native。
    /// </summary>
    public ForworderSource Source { get; init; }

    public static ResponseResult FromOk(ResponseMessage data, ForworderSource source = ForworderSource.None)
    {
        return new ResponseResult() { Data = data, Source = source };
    }

    public static ResponseResult FromOk(int code, ResponseMessage data, ForworderSource source = ForworderSource.None)
    {
        return new ResponseResult() { Code = code, Data = data, Source = source };
    }

    public static ResponseResult FromError(int code, string errMessage, ForworderSource source = ForworderSource.None)
    {
        return new ResponseResult() { Code = code, ErrorMessage = errMessage, Source = source };
    }

    public static ResponseResult FromError(ErrorCode code, string errMessage, ForworderSource source = ForworderSource.None)
    {
        return new ResponseResult() { Code = (int)code, ErrorMessage = errMessage, Source = source };
    }
}
