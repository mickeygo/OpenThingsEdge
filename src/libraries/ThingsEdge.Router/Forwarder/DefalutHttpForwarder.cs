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
        var httpClient = _httpClientFactory.CreateClient("ThingsEdge.Router.RESTfulClient");
        var resp = await httpClient.PostAsJsonAsync("/api/edge", message, cancellationToken);
        if (resp.IsSuccessStatusCode)
        {

        }
        throw new NotImplementedException();
    }
}
