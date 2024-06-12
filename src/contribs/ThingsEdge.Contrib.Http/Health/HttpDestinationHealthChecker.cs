using ThingsEdge.Contrib.Http.Configuration;
using ThingsEdge.Contrib.Http.Model;

namespace ThingsEdge.Contrib.Http.Health;

/// <summary>
/// 目标服务健康检测。
/// </summary>
public sealed class HttpDestinationHealthChecker : IDestinationHealthChecker
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RESTfulDestinationOptions _restfulOptions;

    public HttpDestinationHealthChecker(IHttpClientFactory httpClientFactory, IOptions<RESTfulDestinationOptions> restfulOptions)
    {
        _httpClientFactory = httpClientFactory;
        _restfulOptions = restfulOptions.Value;
    }

    public async Task<DestinationHealthState> CheckAsync(CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(ForwarderConstants.HttpClientName);

        try
        {
            var resp = await httpClient.GetAsync(_restfulOptions.HealthRequestUrl, cancellationToken).ConfigureAwait(false);
            return resp.IsSuccessStatusCode ? DestinationHealthState.Good : DestinationHealthState.Bad;
        }
        catch (Exception)
        {
            return DestinationHealthState.Bad;
        }
    }
}
