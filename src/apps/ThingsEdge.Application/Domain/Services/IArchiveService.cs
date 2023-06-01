namespace ThingsEdge.Application.Domain.Services;

/// <summary>
/// 产品出站服务。
/// </summary>
public interface IArchiveService
{
    /// <summary>
    /// 产品出站存档
    /// </summary>
    /// <param name="line">线体</param>
    /// <param name="station">工站</param>
    /// <param name="sn">SN</param>
    /// <returns></returns>
    Task ArchiveAsync(string line, string station, string sn);
}
