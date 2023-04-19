using ThingsEdge.Router.Configuration;

namespace ThingsEdge.Router.Forwarder;

internal sealed class DefalutHttpForwarder : IHttpForwarder
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    public DefalutHttpForwarder(IHttpClientFactory httpClientFactory, ILogger<DefalutHttpForwarder> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<HttpResult> SendAsync(string requestUri, RequestMessage message, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(ForwarderConstants.HttpClientName);
        int timeout = 5000;
        try
        {
            // 记录请求的数据。
            _logger.LogInformation("RequestId: {RequestId}, 设备: {DeviceName}, 分组: {TagGroupName}, 请求值: {Values}", 
                message.RequestId, message.Schema.DeviceName, message.Schema.TagGroupName, 
                JsonSerializer.Serialize(message.Values.Select(s => new { s.TagName, s.Address, s.DataType, s.Length, s.Value })));

            var cts0 = new CancellationTokenSource(timeout); // 设置超时时间。
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts0.Token);

            // 注：虽然后做超时处理，但数据接收端还是要做幂等处理来预防重复数据。
            var resp = await httpClient.PostAsJsonAsync(requestUri, message, cts.Token).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                return HttpResult.FromError(ErrorCode.HttpResponseError, $"调用 HTTP 服务出错，返回状态码：{resp.StatusCode}");
            }

            var ret = await resp.Content.ReadFromJsonAsync<HttpResponseResult>(cancellationToken: cts.Token).ConfigureAwait(false);
            var respMessage = new ResponseMessage
            {
                Request = message,
                State = ret?.Code ?? 0,
                CallbackItems = ret?.Data ?? new(0),
            };

            // 记录响应的数据
            _logger.LogInformation("RequestId: {RequestId}, 状态: {State}, 回调值: {CallbackItems}",
               message.RequestId, respMessage.State, JsonSerializer.Serialize(respMessage.CallbackItems));

            return HttpResult.FromOk(respMessage);
        }
        catch (OperationCanceledException ex)
        {
            return HttpResult.FromError(ErrorCode.HttpRequestTimedOut, $"请求 HTTP 服务超时，timeout：{timeout}ms，错误：{ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            return HttpResult.FromError(ErrorCode.HttpRequestError, $"调用 HTTP 服务请求异常，错误：{ex.Message}");
        }
        catch (JsonException ex)
        {
            return HttpResult.FromError(ErrorCode.HttpResponseJsonError, $"调用 HTTP 服务解析返回数据异常，错误：{ex.Message}");
        }
        catch (Exception ex)
        {
            return HttpResult.FromError(ErrorCode.HttpError, $"调用 HTTP 服务异常，错误：{ex.Message}");
        }
    }
}
