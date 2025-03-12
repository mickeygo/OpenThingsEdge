using ThingsEdge.ConsoleApp.Handlers;
using ThingsEdge.Exchange.Contracts;
using ThingsEdge.Exchange.Forwarders;

namespace ThingsEdge.ConsoleApp.Forwarders;

/// <summary>
/// 本地转发服务处理接口。
/// </summary>
/// <remarks>一般用于处理主数据。</remarks>
internal sealed class TriggerForwader(IServiceScopeFactory serviceScopeFactory, ILogger<TriggerForwader> logger) : ITriggerForwarder
{
    private readonly ILogger _logger = logger;

    public async Task<ResponseMessage> HandleAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        ResponseMessage response = new()
        {
            Request = message,
        };

        Dictionary<string, Type> map = new() {
            { "PLC_Inbound_Sign", typeof(ArchiveHandler) },
        };

        HandleResult result;
        var self = message.Self();

        if (map.TryGetValue(self.TagName, out var type))
        {
            using var scope = serviceScopeFactory.CreateScope();
            var handler = (AbstractHandler)scope.ServiceProvider.GetRequiredService(type);
            result = await handler.HandleAsync(message, cancellationToken).ConfigureAwait(false);
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
