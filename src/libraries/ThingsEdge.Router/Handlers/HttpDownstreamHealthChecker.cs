﻿using ThingsEdge.Router.Configuration;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 下游健康检测。
/// </summary>
public sealed class HttpDownstreamHealthChecker : IDownstreamHealthChecker
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    public HttpDownstreamHealthChecker(IHttpClientFactory httpClientFactory, ILogger<HttpDownstreamHealthChecker> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<DestinationHealthState> CheckAsync(CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(ForwarderConstants.HttpClientName);

        try
        {
            var resp = await httpClient.GetAsync(ForwarderConstants.HealthRequestUri, cancellationToken);
            return resp.IsSuccessStatusCode ? DestinationHealthState.Good : DestinationHealthState.Bad;
        }
        catch (Exception)
        {
            return DestinationHealthState.Bad;
        }
    }
}