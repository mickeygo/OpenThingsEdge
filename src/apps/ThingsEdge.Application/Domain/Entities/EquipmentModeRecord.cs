namespace ThingsEdge.Application.Domain.Entities;

/// <summary>
/// 设备运行模式记录。
/// </summary>
[SugarTable("equipment_mode_record")]
public sealed class EquipmentModeRecord : EntityBaseId
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
    /// 设备运行模式
    /// </summary>
    public EquipmentRunningMode RunningMode { get; init; }

    /// <summary>
    /// 记录时间
    /// </summary>
    public DateTime RecordTime { get; init; }
}
