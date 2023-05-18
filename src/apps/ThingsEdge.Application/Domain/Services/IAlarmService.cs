namespace ThingsEdge.Application.Domain.Services;

/// <summary>
/// 警报服务。
/// </summary>
public interface IAlarmService : IDomainService
{
    /// <summary>
    /// 记录警报信息。
    /// </summary>
    /// <param name="oldAlarms">上一次的警报集合，没有则为 Empty</param>
    /// <param name="newAlarms">产生的警报集合</param>
    /// <returns></returns>
    Task RecordAlarmsAsync(bool[] oldAlarms, bool[] newAlarms);
}
