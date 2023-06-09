﻿using ThingsEdge.Router.Configuration;

namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// 基于 HTTP 协议的转发数据。
/// </summary>
internal sealed class HttpForwarder : IForwarder
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RESTfulDestinationOptions _restfulOptions;
    private readonly ILogger _logger;

    public HttpForwarder(IHttpClientFactory httpClientFactory, 
        IOptionsMonitor<RESTfulDestinationOptions> restfulOptions, 
        ILogger<HttpForwarder> logger)
    {
        _httpClientFactory = httpClientFactory;
        _restfulOptions = restfulOptions.CurrentValue;
        _logger = logger;
    }

    public ForworderSource Source => ForworderSource.HTTP;

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
            var requestUri = _restfulOptions.RequestUrlFunc?.Invoke(new()
            {
                Schema = message.Schema,
                Flag = message.Flag,
                TagName = message.Self().TagName,
            }) ?? _restfulOptions.RequestUrl;

            // 注：虽然后做超时处理，但数据接收端还是要做幂等处理来预防重复数据。
            var resp = await httpClient.PostAsJsonAsync(requestUri, message, cancellationToken).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                return ResponseResult.FromError(ErrorCode.HttpResponseError, $"调用 HTTP 服务出错，返回状态码：{resp.StatusCode}", Source);
            }

            var ret = await resp.Content.ReadFromJsonAsync<HttpResponseResult>(cancellationToken: cancellationToken).ConfigureAwait(false);
            var respMessage = new ResponseMessage
            {
                Request = message,
                State = ret?.Code ?? 0,
                CallbackItems = ret?.Data,
            };

            // 记录响应的数据
            _logger.LogInformation("RequestId: {RequestId}, 状态: {State}, 回调值: {CallbackItems}",
               message.RequestId, respMessage.State, JsonSerializer.Serialize(respMessage.CallbackItems));

            return ResponseResult.FromOk(respMessage, Source);
        }
        catch (OperationCanceledException ex)
        {
            return ResponseResult.FromError(ErrorCode.HttpRequestTimedOut, $"请求 HTTP 服务超时，错误：{ex.Message}", Source);
        }
        catch (HttpRequestException ex)
        {
            return ResponseResult.FromError(ErrorCode.HttpRequestError, $"调用 HTTP 服务请求异常，错误：{ex.Message}", Source);
        }
        catch (JsonException ex)
        {
            return ResponseResult.FromError(ErrorCode.HttpResponseJsonError, $"调用 HTTP 服务解析返回数据异常，错误：{ex.Message}", Source);
        }
        catch (Exception ex)
        {
            return ResponseResult.FromError(ErrorCode.HttpError, $"调用 HTTP 服务异常，错误：{ex.Message}", Source);
        }
    }
}
