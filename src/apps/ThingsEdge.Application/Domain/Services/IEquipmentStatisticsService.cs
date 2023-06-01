namespace ThingsEdge.Application.Domain.Services;

/// <summary>
/// 设备数据统计服务
/// </summary>
public interface IEquipmentStatisticsService
{
    /// <summary>
    /// OEE 分析
    /// </summary>
    /// <param name="query">查询条件</param>
    /// <returns></returns>
    Task<OEECollectionDto> AnalysisOEEAsync(OEEQueryInput query);
}
