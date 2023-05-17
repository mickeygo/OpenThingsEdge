namespace ThingsEdge.Router.Interfaces;

/// <summary>
/// 设备请求预处理接口。
/// </summary>
public interface IMessageRequestPostingApi
{
    /// <summary>
    /// 消息请求
    /// </summary>
    /// <param name="oldPayloadData"></param>
    /// <param name="requestMessage">请求消息</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PostAsync(PayloadData? oldPayloadData, RequestMessage requestMessage, CancellationToken cancellationToken);
}
