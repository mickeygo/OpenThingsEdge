using ThingsEdge.Contracts;
using ThingsEdge.Exchange.Common.DependencyInjection;

namespace ThingsEdge.App.Handlers;

public sealed class ArchiveHandler(ILogger<ArchiveHandler> logger) : AbstractHandler, ITransientDependency
{
    public override Task<HandleResult> HandleAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("数据存档处理，数据：{@Value}", message.Values.Select(s => new { s.Address, s.Value }));

        return Task.FromResult(HandleResult.Ok());
    }
}
