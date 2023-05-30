using ThingsEdge.Application.Dtos;

namespace ThingsEdge.Application.Domain.Services;

/// <summary>
/// 设备数据统计服务
/// </summary>
public interface IEquipmentStatisticsService : IDomainService
{
    /// <summary>
    /// OEE 分析
    /// </summary>
    /// <param name="query">查询</param>
    /// <returns></returns>
    Task<OEEDto> AnalysisOEEAsync(OEEQueryInput query);
}
