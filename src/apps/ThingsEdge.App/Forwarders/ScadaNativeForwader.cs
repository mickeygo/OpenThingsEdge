using ThingsEdge.Contracts;
using ThingsEdge.Contracts.Variables;
using ThingsEdge.App.Handlers;
using ThingsEdge.Router.Forwarder;

namespace ThingsEdge.App.Forwarders;

/// <summary>
/// 本地转发服务接口。
/// </summary>
/// <remarks>一般用于处理主数据。</remarks>
internal sealed class ScadaNativeForwader : INativeForwarder
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public ScadaNativeForwader(IServiceProvider serviceProvider, ILogger<ScadaNativeForwader> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<ResponseMessage> SendAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        ResponseMessage response = new()
        {
            Request = message,
        };

        // 只处理 Trigger 触发数据。
        if (message.Flag != TagFlag.Trigger)
        {
            return response;
        }

        Dictionary<string, Type> map = new() { 
            { "PLC_Archive_Sign", typeof(ArchiveHandler) },
        };

        HandleResult result;
        var self = message.Self();

        if (map.TryGetValue(self.TagName, out var typ))
        {
            using var scope = _serviceProvider.CreateScope();
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
