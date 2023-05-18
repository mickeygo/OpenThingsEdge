using ThingsEdge.Application.Models;

namespace ThingsEdge.Application.Domain.Entities;

/// <summary>
/// 设备运行模式记录。
/// </summary>
[SugarTable("EquipmentModeRecord")]
public sealed class EquipmentModeRecord : EntityBaseId
{
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
