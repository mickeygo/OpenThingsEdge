namespace ThingsEdge.Application.Domain.Services;

/// <summary>
/// 产品进站服务
/// </summary>
internal interface IEntryService : IDomainService
{
    /// <summary>
    /// 产品进站请求
    /// </summary>
    /// <param name="line">线体</param>
    /// <param name="station">工站</param>
    /// <param name="sn">SN</param>
    /// <returns></returns>
    Task EntryAsync(string line, string station, string sn);
}
