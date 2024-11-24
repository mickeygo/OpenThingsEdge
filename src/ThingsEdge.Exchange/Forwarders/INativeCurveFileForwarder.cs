using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 曲线文件信息发送接口，其中 <see cref="TagFlag.Switch"/> 会发布此事件 。
/// </summary>
public interface INativeCurveFileForwarder
{
    Task PostAsync(CurveFilePostedEvent info, CancellationToken cancellationToken);
}
