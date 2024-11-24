using ThingsEdge.App.Handlers;
using ThingsEdge.Contracts;
using ThingsEdge.Exchange.Forwarders;

namespace ThingsEdge.App.Forwarders;

/// <summary>
/// 本地转发服务处理接口。
/// </summary>
/// <remarks>一般用于处理主数据。</remarks>
internal sealed class ScadaRequestForwaderHandler : IRequestForwarderHandler
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger _logger;

    public ScadaRequestForwaderHandler(IServiceScopeFactory serviceScopeFactory, ILogger<ScadaRequestForwaderHandler> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task<ResponseMessage> HandleAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        ResponseMessage response = new()
        {
            Request = message,
        };

        Dictionary<string, Type> map = new() { 
            { "PLC_Archive_Sign", typeof(ArchiveHandler) },
        };

        HandleResult result;
        var self = message.Self();

        if (map.TryGetValue(self.TagName, out var typ))
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var handler = (AbstractHandler)scope.ServiceProvider.GetRequiredService(typ);
            result = await handler.HandleAsync(message, cancellationToken);
        }
        else
        {
            _logger.LogWarning("请求的标记名称 {TagName} 必须属于 {@Tags} 其中的一种。", self.TagName, map.Keys);
            result = new() { State = 0 };
        }

        response.State = result.State;
        response.CallbackItems = result.CallbackItems;
        return response;
    }
}
