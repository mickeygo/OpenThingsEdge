using ThingsEdge.Router.Events;
using ThingsEdge.Router.Interfaces;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 曲线文件信息处理器。
/// </summary>
internal sealed class CurveFilePostedHandler : INotificationHandler<CurveFilePostedEvent>
{
    private readonly IServiceProvider _serviceProvider;

    public CurveFilePostedHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Handle(CurveFilePostedEvent notification, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var curveFilePostedApi = scope.ServiceProvider.GetService<ICurveFilePostedApi>();
        if (curveFilePostedApi != null)
        {
            await curveFilePostedApi.PostAsync(notification, cancellationToken).ConfigureAwait(false);
        }
    }
}
