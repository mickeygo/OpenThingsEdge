using ThingsEdge.Exchange.Contracts;

namespace ThingsEdge.ConsoleApp.Handlers;

/// <summary>
/// 
/// </summary>
/// <param name="logger"></param>
public sealed class ArchiveHandler(ILogger<ArchiveHandler> logger) : AbstractHandler
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task<HandleResult> HandleAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("数据存档处理，数据：{@Value}", message.Values.Select(s => new { s.Address, s.Value }));

        return Task.FromResult(HandleResult.Ok());
    }
}
