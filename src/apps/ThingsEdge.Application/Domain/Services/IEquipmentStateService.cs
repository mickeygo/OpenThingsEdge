namespace ThingsEdge.Application.Domain.Services;

/// <summary>
/// 设备运行状态服务。
/// </summary>
public interface IEquipmentStateService
{
    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="query">筛选条件</param>
    /// <returns></returns>
    Task<PagedList<EquipmentStateRecord>> GetPagedAsync(PagedQuery query);

    /// <summary>
    /// 更改设备运行状态。
    /// </summary>
    /// <param name="line">线体</param>
    /// <param name="equipmentCode">设备编号</param>
    /// <param name="runningState">运行状态</param>
    /// <returns></returns>
    Task ChangeStateAsync(string line, string equipmentCode, EquipmentRunningState runningState);
}
