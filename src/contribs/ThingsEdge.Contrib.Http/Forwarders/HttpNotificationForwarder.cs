using ThingsEdge.Contrib.Http.Configuration;
using ThingsEdge.Router.Forwarders;

namespace ThingsEdge.Contrib.Http.Forwarders;

/// <summary>
/// 基于 HTTP 协议的转发通知服务。
/// </summary>
internal sealed class HttpNotificationForwarder : INotificationForwarder
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RESTfulDestinationOptions _restfulOptions;
    private readonly ILogger _logger;

    public HttpNotificationForwarder(IHttpClientFactory httpClientFactory,
        IOptions<RESTfulDestinationOptions> restfulOptions,
        ILogger<HttpNotificationForwarder> logger)
    {
        _httpClientFactory = httpClientFactory;
        _restfulOptions = restfulOptions.Value;
        _logger = logger;
    }

    public async Task PublishAsync(RequestMessage message, PayloadData? lastMasterPayloadData, CancellationToken cancellationToken = default)
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
            var jsonContent = JsonContent.Create(value, value.GetType());
            var resp = await httpClient.PostAsync(requestUri, jsonContent, cancellationToken).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("调用 HTTP 服务出错，返回状态码：{StatusCode}", resp.StatusCode);
                return;
            }

            _logger.LogInformation("RequestId: {RequestId}, 调用 HTTP 通知数据完成", message.RequestId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("[HttpNotificationForwarder] 调用 HTTP 服务超时");
        }
        catch (HttpRequestException)
        {
            _logger.LogWarning("[HttpNotificationForwarder] 调用 HTTP 服务解析返回数据异常");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[HttpNotificationForwarder] 调用 HTTP 服务异常");
        }
    }
}
