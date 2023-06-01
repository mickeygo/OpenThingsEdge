namespace ThingsEdge.Application.Domain.Services;

/// <summary>
/// 警报设置服务
/// </summary>
public interface IAlarmSettingService
{
    /// <summary>
    /// 获取全部的警报对象。
    /// </summary>
    /// <returns></returns>
    Task<List<AlarmSetting>> GetAllAsync();

    /// <summary>
    /// 通过Id获取警报设置对象
    /// </summary>
    /// <param name="id">警报Id</param>
    /// <returns></returns>
    Task<AlarmSetting?> GetByIdAsync(long id);

    /// <summary>
    /// 通过编号集合获取警报设置对象集合
    /// </summary>
    /// <param name="noList">编号集合</param>
    /// <returns></returns>
    Task<List<AlarmSetting>> GetByNoAsync(IEnumerable<int> noList);

    /// <summary>
    /// 新增或更新的警报
    /// </summary>
    /// <param name="alarmSetting">要新增或更新的警报设置对象</param>
    /// <returns></returns>
    Task<(bool ok, string? err)> InsertOrUpdateAsync(AlarmSetting alarmSetting);

    /// <summary>
    /// 删除警报
    /// </summary>
    /// <param name="alarmSetting">要删除的警报设置对象</param>
    /// <returns></returns>
    Task DeleteAsync(AlarmSetting alarmSetting);
}
