using ThingsEdge.Router.Model;

namespace ThingsEdge.Router.Handlers.Health;

/// <summary>
/// 健康检查处理策略。
/// </summary>
public interface IHealthCheckHandlePolicy
{
    /// <summary>
    /// 处理检查结果。
    /// </summary>
    /// <param name="healthState">健康状态</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HandleAsync(DestinationHealthState healthState, CancellationToken cancellationToken);
}
