namespace ThingsEdge.Router.Forwarder;

public sealed class HealthHttpForwarder
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    public HealthHttpForwarder(IHttpClientFactory httpClientFactory, ILogger<HealthHttpForwarder> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<bool> CheckAsync(CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient("ThingsEdge.Router.RESTfulClient");
       
        try
        {
            var resp = await httpClient.GetAsync("/api/health", cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                return false;
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
