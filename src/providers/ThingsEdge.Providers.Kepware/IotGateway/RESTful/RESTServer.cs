namespace Ops.Contrib.Kepware.IotGateway.RESTful;

/// <summary>
/// REST 服务端
/// </summary>
internal sealed class RESTServer : IRESTServerApi
{
    public const string HttpClientName = "IotGatewayRESTServerApi";

    private readonly IHttpClientFactory _httpClientFactory = null!;
    private readonly ILogger _logger = null!;

    public RESTServer(IHttpClientFactory httpClientFactory, ILogger<RESTServer> logger)
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
            return await httpClient.GetFromJsonAsync<ReadResult>(requestUri, cancellationToken);
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
            var resp = await httpClient.PostAsJsonAsync("/write", value0, cancellationToken);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<WriteResultCollection>();
        }
        catch (Exception)
        {
            throw;
        }
    }
}
