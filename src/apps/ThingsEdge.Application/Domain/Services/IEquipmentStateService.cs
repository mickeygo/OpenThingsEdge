using ThingsEdge.Application.Models;

namespace ThingsEdge.Application.Domain.Services;

/// <summary>
/// 设备运行状态服务。
/// </summary>
public interface IEquipmentStateService : IDomainService
{
    /// <summary>
    /// 更改设备运行状态。
    /// </summary>
    /// <param name="line">线体</param>
    /// <param name="equipmentCode">设备编号</param>
    /// <param name="runningState">运行状态</param>
    /// <returns></returns>
    Task ChangeStateAsync(string line, string equipmentCode, EquipmentRunningState runningState);
}
