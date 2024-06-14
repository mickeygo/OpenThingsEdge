using ThingsEdge.Router.Events;
using ThingsEdge.Router.Forwarders;

namespace ThingsEdge.Router.Handlers;

/// <summary>
/// 曲线文件信息处理器。
/// </summary>
internal sealed class CurveFilePostedHandler(IServiceProvider serviceProvider) : INotificationHandler<CurveFilePostedEvent>
{
    public async Task Handle(CurveFilePostedEvent notification, CancellationToken cancellationToken)
    {
        var forwarder = serviceProvider.GetService<INativeCurveFileForwarder>();
        if (forwarder != null)
        {
            await forwarder.PostAsync(notification, cancellationToken).ConfigureAwait(false);
        }
    }
}
