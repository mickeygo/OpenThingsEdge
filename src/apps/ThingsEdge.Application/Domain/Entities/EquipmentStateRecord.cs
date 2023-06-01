namespace ThingsEdge.Application.Domain.Entities;

/// <summary>
/// 设备状态记录。
/// </summary>
[SugarTable("equipment_state_record")]
public sealed class EquipmentStateRecord : EntityBaseId
{
    /// <summary>
    /// 产线
    /// </summary>
    [NotNull]
    public string? Line { get; init; }

    /// <summary>
    /// 设备编号
    /// </summary>
    [NotNull]
    public string? EquipmentCode { get; init; }

    /// <summary>
    /// 设备名称
    /// </summary>
    [NotNull]
    public string? EquipmentName { get; init; }

    /// <summary>
    /// 设备运行状态
    /// </summary>
    public EquipmentRunningState RunningState { get; init; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; init; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 该状态是否已结束。
    /// </summary>
    public bool IsEnded { get; set; }

    /// <summary>
    /// 持续时长，单位秒。
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// 闭合
    /// </summary>
    public void Close()
    {
        IsEnded = true;
        EndTime = DateTime.Now;
        Duration = Convert.ToInt32((EndTime - StartTime).Value.TotalSeconds);
    }
}
