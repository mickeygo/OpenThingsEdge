using ThingsEdge.Exchange.Contracts;
using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Engine.Policies;

/// <summary>
/// 监控策略
/// </summary>
public interface IMonitorPolicy
{
    /// <summary>
    /// 验证数据是否通过。
    /// </summary>
    /// <param name="data">从设备中取到的数据</param>
    /// <returns></returns>
    bool Validate(PayloadData data) => true;

    /// <summary>
    /// 写入给设备的数据，返回 null 表示不写入。
    /// </summary>
    /// <param name="tag">要写入的设备标记</param>
    /// <returns></returns>
    object? Return(Tag tag) => null;
}
