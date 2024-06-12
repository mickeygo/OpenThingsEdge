using ThingsEdge.Contracts;
using ThingsEdge.Router.Interfaces;

namespace ThingsEdge.App.Handlers;

/// <summary>
/// 设备消息请求预处理，用于异步处理通知消息，如警报、设备状态信息。
/// </summary>
internal sealed class MessageApiHandler : IDirectMessageRequestApi
{
    private readonly ILogger _logger;

    public MessageApiHandler(ILogger<MessageApiHandler> logger)
    {
        _logger = logger;
    }

    public Task PostAsync(PayloadData? lastMasterPayloadData, RequestMessage requestMessage, CancellationToken cancellationToken)
    {
        _logger.LogInformation("MessageApiHandler");

        return Task.CompletedTask;
    }
}
