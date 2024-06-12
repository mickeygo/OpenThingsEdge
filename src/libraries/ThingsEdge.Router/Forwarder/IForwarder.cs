namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// 数据传送请求，接收处理后有响应结果。
/// </summary>
public interface IForwarder
{
    /// <summary>
    /// 转发请求源分类
    /// </summary>
    ForworderSource Source { get; }

    /// <summary>
    /// 发送数据。
    /// </summary>
    /// <param name="message">要发送的数据。</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResponseResult> SendAsync(RequestMessage message, CancellationToken cancellationToken = default);
}
