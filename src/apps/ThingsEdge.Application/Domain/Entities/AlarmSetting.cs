namespace ThingsEdge.Application.Domain.Entities;

/// <summary>
/// 警报信息设定。
/// </summary>
[SugarTable("alarm_setting")]
public sealed class AlarmSetting : EntityBase
{
    /// <summary>
    /// 警报编号，从1开始。
    /// </summary>
    public int No { get; set; }

    /// <summary>
    /// 警报消息
    /// </summary>
    [NotNull]
    public string? Message { get; set; }
}
