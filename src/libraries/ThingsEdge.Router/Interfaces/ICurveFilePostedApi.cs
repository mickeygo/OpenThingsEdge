using ThingsEdge.Router.Events;

namespace ThingsEdge.Router.Interfaces;

/// <summary>
/// 曲线文件信息发送接口，其中 <see cref="TagFlag.Switch"/> 会发布此事件 。
/// </summary>
public interface ICurveFilePostedApi
{
    Task PostAsync(CurveFilePostedEvent info, CancellationToken cancellationToken);
}
