namespace ThingsEdge.Providers.Ops.Exchange.Policies;

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
    /// 写入给设备的数据，null 表示不写入。
    /// </summary>
    /// <param name="data">要写入设备的数据</param>
    /// <returns></returns>
    object? Return(PayloadData data) => null;
}
