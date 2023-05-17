namespace ThingsEdge.Application.Domain.Services;

/// <summary>
/// 警报服务。
/// </summary>
public interface IAlarmService
{
    /// <summary>
    /// 记录警报信息。
    /// </summary>
    /// <param name="alarms">产生的警报集合</param>
    /// <returns></returns>
    Task<(bool ok, string? err)> RecordAlarmsAsync(bool[] alarms);
}
