using ThingsEdge.Contrib.Http.Model;

namespace ThingsEdge.Contrib.Http.Health;

/// <summary>
/// 下游系统健康检测。
/// </summary>
public interface IDestinationHealthChecker
{
    /// <summary>
    /// 检查下游服务的健康状况。
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<DestinationHealthState> CheckAsync(CancellationToken cancellationToken);
}
