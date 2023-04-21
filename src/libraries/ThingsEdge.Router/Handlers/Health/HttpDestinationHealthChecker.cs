using ThingsEdge.Router.Configuration;
using ThingsEdge.Router.Model;

namespace ThingsEdge.Router.Handlers.Health;

/// <summary>
/// 目标服务健康检测。
/// </summary>
public sealed class HttpDestinationHealthChecker : IDestinationHealthChecker
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpDestinationHealthChecker(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<DestinationHealthState> CheckAsync(CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(ForwarderConstants.HttpClientName);

        try
        {
            var resp = await httpClient.GetAsync(ForwarderConstants.HealthRequestUri, cancellationToken).ConfigureAwait(false);
            return resp.IsSuccessStatusCode ? DestinationHealthState.Good : DestinationHealthState.Bad;
        }
        catch (Exception)
        {
            return DestinationHealthState.Bad;
        }
    }
}
