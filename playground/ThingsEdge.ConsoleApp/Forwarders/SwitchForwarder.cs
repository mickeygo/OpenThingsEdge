using ThingsEdge.Exchange.Forwarders;

namespace ThingsEdge.ConsoleApp.Forwarders;

/// <summary>
/// 开关消息处理。
/// </summary>
/// <remarks>一般用于处理曲线数据。</remarks>
internal sealed class SwitchForwarder(ILogger<SwitchForwarder> logger) : ISwitchForwarder
{
    public Task ReceiveAsync(SwitchContext context, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("通知消息处理，数据：{@Value}", context.Message.Values.Select(s => new { s.Address, s.Value }));
        logger.LogInformation("曲线文件：{FilePath}", context.FilePath);

        return Task.CompletedTask;
    }
}
