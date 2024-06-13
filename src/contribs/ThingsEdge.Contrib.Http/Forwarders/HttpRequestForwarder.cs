using ThingsEdge.Contrib.Http.Configuration;
using ThingsEdge.Router.Forwarders;

namespace ThingsEdge.Contrib.Http.Forwarders;

/// <summary>
/// 基于 HTTP 协议的转发请求服务。
/// </summary>
internal sealed class HttpRequestForwarder : IRequestForwarder
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RESTfulDestinationOptions _restfulOptions;
    private readonly ILogger _logger;

    public HttpRequestForwarder(IHttpClientFactory httpClientFactory, 
        IOptions<RESTfulDestinationOptions> restfulOptions, 
        ILogger<HttpRequestForwarder> logger)
    {
        _httpClientFactory = httpClientFactory;
        _restfulOptions = restfulOptions.Value;
        _logger = logger;
    }

    public async Task<ResponseResult> SendAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(ForwarderConstants.HttpClientName);
        try
        {
            // 记录请求的数据。
            _logger.LogInformation("RequestId: {RequestId}, 设备: {DeviceName}, 分组: {TagGroupName}, 请求值: {Values}",
                message.RequestId, message.Schema.DeviceName, message.Schema.TagGroupName,
                JsonSerializer.Serialize(message.Values.Select(s => new { s.TagName, s.Address, s.DataType, s.Length, s.Value })));

            // 路由适配
            var requestUri = _restfulOptions.RequestUrlFormaterFunc?.Invoke(new()
            {
                Schema = message.Schema,
                Flag = message.Flag,
                TagName = message.Self().TagName,
            }) ?? _restfulOptions.RequestUrl;
            var value = _restfulOptions.RequestJsonValueFormaterFunc?.Invoke(message) ?? message;

            // 注：虽然后做超时处理，但数据接收端还是要做幂等处理来预防重复数据。
            var jsonContent = JsonContent.Create(value, value.GetType());
            var resp = await httpClient.PostAsync(requestUri, jsonContent, cancellationToken).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                return ResponseResult.FromError(ErrorCode.HttpResponseError, $"调用 HTTP 服务出错，返回状态码：{resp.StatusCode}");
            }

            var ret = await resp.Content.ReadFromJsonAsync<HttpResponseResult>(cancellationToken).ConfigureAwait(false);
            var respMessage = new ResponseMessage
            {
                Request = message,
                State = ret?.Code ?? 0,
                CallbackItems = ret?.Data,
            };

            // 记录响应的数据
            _logger.LogInformation("RequestId: {RequestId}, 状态: {State}, 回调值: {CallbackItems}",
               message.RequestId, respMessage.State, JsonSerializer.Serialize(respMessage.CallbackItems));

            return ResponseResult.FromOk(respMessage);
        }
        catch (OperationCanceledException ex)
        {
            return ResponseResult.FromError(ErrorCode.HttpRequestTimedOut, $"[HttpNotificationForwarder] 请求 HTTP 服务超时，错误：{ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            return ResponseResult.FromError(ErrorCode.HttpRequestError, $"[HttpNotificationForwarder] 调用 HTTP 服务请求异常，错误：{ex.Message}");
        }
        catch (JsonException ex)
        {
            return ResponseResult.FromError(ErrorCode.HttpResponseJsonError, $"[HttpNotificationForwarder] 调用 HTTP 服务解析返回数据异常，错误：{ex.Message}");
        }
        catch (Exception ex)
        {
            return ResponseResult.FromError(ErrorCode.HttpError, $"[HttpNotificationForwarder] 调用 HTTP 服务异常，错误：{ex.Message}");
        }
    }
}
