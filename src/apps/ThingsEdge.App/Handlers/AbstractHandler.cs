using ThingsEdge.Contracts;

namespace ThingsEdge.App.Handlers;

/// <summary>
/// Handler 基类。
/// </summary>
public abstract class AbstractHandler
{
    /// <summary>
    /// 处理请求数据
    /// </summary>
    /// <param name="message">请求消息</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public abstract Task<HandleResult> HandleAsync(RequestMessage message, CancellationToken cancellationToken = default);
}
