namespace ThingsEdge.Application.Management.Equipment;

/// <summary>
/// 设备运行状态
/// </summary>
public enum EquipmentRunningState
{
    /// <summary>
    /// 运行中
    /// </summary>
    Running = 1,

    /// <summary>
    /// 警报中
    /// </summary>
    Warning,

    /// <summary>
    /// 急停中
    /// </summary>
    EmergencyStopping,

    /// <summary>
    /// 离线
    /// </summary>
    Offline,
}