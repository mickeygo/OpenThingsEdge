namespace ThingsEdge.Router.Interfaces;

/// <summary>
/// 设备请求预处理接口，其中 <see cref="TagFlag.Notice"/>、<see cref="TagFlag.Trigger"/> 和 <see cref="TagFlag.Switch"/> 都会发布此事件。
/// </summary>
public interface IDirectMessageRequestApi
{
    /// <summary>
    /// 请求消息。
    /// </summary>
    /// <param name="lastMasterPayloadData">最近一次记录的标记主数据，没有则为 null</param>
    /// <param name="requestMessage">请求消息</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PostAsync(PayloadData? lastMasterPayloadData, RequestMessage requestMessage, CancellationToken cancellationToken);
}
