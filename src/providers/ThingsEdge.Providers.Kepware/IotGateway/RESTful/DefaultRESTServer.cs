namespace ThingsEdge.Providers.Kepware.IotGateway.RESTful;

/// <summary>
/// REST 服务端
/// </summary>
internal sealed class DefaultRESTServer : IRESTServerApi
{
    public const string HttpClientName = "KepwareIotGatewayRESTServerApi";

    private readonly IHttpClientFactory _httpClientFactory = null!;
    private readonly ILogger _logger = null!;

    public DefaultRESTServer(IHttpClientFactory httpClientFactory, ILogger<DefaultRESTServer> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<ReadResult?> ReadAsync(string[] tags, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(HttpClientName);
            var requestUri = $"/read/{string.Join("&", tags.Select(s => $"ids={s}"))}";
            return await httpClient.GetFromJsonAsync<ReadResult>(requestUri, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<WriteResultCollection?> WriteAsync(Dictionary<string, object> value, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(HttpClientName);
            var value0 = value.Select(s => new { id = s.Key, v = s.Value });
            var resp = await httpClient.PostAsJsonAsync("/write", value0, cancellationToken).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<WriteResultCollection>(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
