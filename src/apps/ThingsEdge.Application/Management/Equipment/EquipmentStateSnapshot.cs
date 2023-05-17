namespace ThingsEdge.Application.Management.Equipment;

/// <summary>
/// 设备状态快照。
/// </summary>
public sealed class EquipmentStateSnapshot
{
    /// <summary>
    /// 设备编号
    /// </summary>
    [NotNull]
    public string? EquipmentCode { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    [NotNull]
    public string? EquipmentName { get; set; }

    /// <summary>
    /// 设备运行模式
    /// </summary>
    public EquipmentRunningMode RunningMode { get; set; }

    /// <summary>
    /// 设备运行状态
    /// </summary>
    public EquipmentRunningState RunningState { get; set; }
}
