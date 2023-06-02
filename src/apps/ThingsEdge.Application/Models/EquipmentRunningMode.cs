namespace ThingsEdge.Application.Models;

/// <summary>
/// 设备运行模式（状态互斥）
/// </summary>
public enum EquipmentRunningMode
{
    /// <summary>
    /// 未知
    /// </summary>
    [Display(Name = "未知")]
    Unkown = 0,

    /// <summary>
    /// 手动模式
    /// </summary>
    [Display(Name = "手动")]
    Manual = 1,

    /// <summary>
    /// 自动模式
    /// </summary>
    [Display(Name = "自动")]
    Auto = 2
}
