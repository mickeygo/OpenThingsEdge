using ThingsEdge.Application.Dtos;
using ThingsEdge.Application.Models;

namespace ThingsEdge.Application.Domain.Services;

/// <summary>
/// 设备运行模式服务。
/// </summary>
public interface IEquipmentModeService : IDomainService
{
    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="query">筛选条件</param>
    /// <returns></returns>
    Task<PagedList<EquipmentModeRecord>> GetPagedAsync(PagedQuery query);

    /// <summary>
    /// 更改设备运行模式。
    /// </summary>
    /// <param name="line">线体</param>
    /// <param name="equipmentCode">设备编号</param>
    /// <param name="runningMode">运行模式</param>
    /// <returns></returns>
    Task ChangeModeAsync(string line, string equipmentCode, EquipmentRunningMode runningMode);
}
