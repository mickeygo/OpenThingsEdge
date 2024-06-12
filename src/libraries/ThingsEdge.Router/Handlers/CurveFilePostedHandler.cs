using ThingsEdge.Router.Events;
using ThingsEdge.Router.Interfaces;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 曲线文件信息处理器。
/// </summary>
internal sealed class CurveFilePostedHandler(IServiceScopeFactory serviceScopeFactory) : INotificationHandler<CurveFilePostedEvent>
{
    public async Task Handle(CurveFilePostedEvent notification, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var curveFilePostedApi = scope.ServiceProvider.GetService<ICurveFilePostedApi>();
        if (curveFilePostedApi != null)
        {
            await curveFilePostedApi.PostAsync(notification, cancellationToken).ConfigureAwait(false);
        }
    }
}
