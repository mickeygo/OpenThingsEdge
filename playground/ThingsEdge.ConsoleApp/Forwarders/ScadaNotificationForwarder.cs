using ThingsEdge.Contracts;
using ThingsEdge.Exchange.Forwarders;

namespace ThingsEdge.App.Forwarders;

/// <summary>
/// 通知消息处理。
/// </summary>
/// <remarks>一般用于处理通知消息数据，如警报、设备状态信息。</remarks>
internal sealed class ScadaNotificationForwarder(ILogger<ScadaNotificationForwarder> logger) : INotificationForwarder
{
    public Task PublishAsync(RequestMessage message, PayloadData? lastMasterPayloadData, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("通知消息处理，数据：{@Value}", message.Values.Select(s => new { s.Address, s.Value }));

        return Task.CompletedTask;
    }
}
