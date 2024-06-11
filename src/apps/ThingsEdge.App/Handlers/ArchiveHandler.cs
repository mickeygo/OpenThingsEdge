using ThingsEdge.Common.DependencyInjection;
using ThingsEdge.Contracts;

namespace ThingsEdge.App.Handlers;

public sealed class ArchiveHandler : AbstractHandler, ITransientDependency
{
    public override Task<HandleResult> HandleAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HandleResult.Ok());
    }
}
