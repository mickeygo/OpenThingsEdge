using ThingsEdge.Router.Events;

namespace ThingsEdge.Router.Interfaces;

/// <summary>
/// 曲线文件信息发送接口，其中 <see cref="TagFlag.Switch"/> 会发布此事件 。
/// </summary>
/// <remarks>服务端采用 <see cref="ServiceLifetime.Scoped"/> 作用域来解析服务。</remarks>
public interface ICurveFilePostedApi
{
    Task PostAsync(CurveFilePostedEvent info, CancellationToken cancellationToken);
}
