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

    public async Task<HttpResult> SendAsync(RequestMessage message, CancellationToken cancellationToken)
    {
        var httpResult = new HttpResult();
        var httpClient = _httpClientFactory.CreateClient("ThingsEdge.Router.RESTfulClient");

        try
        {
            var resp = await httpClient.PostAsJsonAsync("/api/iotgateway", message, cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                httpResult.Code = 2;
                httpResult.ErrorMessage = $"调用 HTTP 服务出错，返回状态码：{resp.StatusCode}";
                return httpResult;
            }

            var ret = await resp.Content.ReadFromJsonAsync<Dictionary<string, object>>(cancellationToken: cancellationToken);
            httpResult.Data = new ResponseMessage
            {
                Schema = message.Schema,
                CallbackItems = ret,
            };
        }
        catch (Exception ex)
        {
            httpResult.Code = 2;
            httpResult.ErrorMessage = $"调用 HTTP 服务出错，错误：{ex.Message}";
        }

        return httpResult;
    }
}
