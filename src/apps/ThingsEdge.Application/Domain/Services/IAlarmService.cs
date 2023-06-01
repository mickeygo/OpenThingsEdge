namespace ThingsEdge.Application.Domain.Services;

/// <summary>
/// 警报服务。
/// </summary>
public interface IAlarmService
{
    /// <summary>
    /// 记录警报信息。
    /// </summary>
    /// <param name="AlarmInput">警报数据</param>
    Task RecordAlarmsAsync(AlarmInput input);
}
