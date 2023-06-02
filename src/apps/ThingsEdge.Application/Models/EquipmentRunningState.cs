namespace ThingsEdge.Application.Models;

/// <summary>
/// 设备运行状态
/// </summary>
public enum EquipmentRunningState
{
    /// <summary>
    /// 运行中
    /// </summary>
    [Display(Name = "运行")]
    Running = 1,

    /// <summary>
    /// 警报中
    /// </summary>
    [Display(Name = "警报")]
    Warning,

    /// <summary>
    /// 急停中
    /// </summary>
    [Display(Name = "急停")]
    EmergencyStopping,

    /// <summary>
    /// 离线（设备与PLC断开）
    /// </summary>
    [Display(Name = "离线")]
    Offline,
}
