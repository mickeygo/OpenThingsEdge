namespace ThingsEdge.Router.Forwarder;

internal sealed class InternalForwarderFactory : IForwarderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public InternalForwarderFactory(IServiceProvider serviceProvider, ILogger<InternalForwarderFactory> logger) 
        => (_serviceProvider, _logger) = (serviceProvider, logger);

    public async Task<ResponseResult> SendAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            var forwarders = InternalForwarderHub.Instance.ResloveAll(_serviceProvider);
            if (!forwarders.Any())
            {
                // 处理 HTTP 返回的状态
                return ResponseResult.FromOk(new ResponseMessage
                {
                    Request = message,
                });
            }

            var tasks = forwarders.Select(s => s.SendAsync(message, cancellationToken));
            var results = await Task.WhenAll(tasks).ConfigureAwait(false); // 等待所有任务结束

            // 只处理 HTTP 请求的返回的状态
            var httpResult = results.FirstOrDefault(s => s.Source == ForworderSource.HTTP);
            if (httpResult is not null)
            {
                return httpResult;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[InternalForwarderFactory] 数据发送异常。");
        }

        // 返回空的状态
        return ResponseResult.FromOk(new ResponseMessage
        {
            Request = message,
        });
    }
}
