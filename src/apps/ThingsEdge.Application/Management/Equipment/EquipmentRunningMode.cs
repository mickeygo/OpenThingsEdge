namespace ThingsEdge.Application.Management.Equipment;

/// <summary>
/// 设备运行模式（状态互斥）
/// </summary>
public enum EquipmentRunningMode
{
    /// <summary>
    /// 未知
    /// </summary>
    Unkown = 0,

    /// <summary>
    /// 手动模式
    /// </summary>
    Manual = 1,

    /// <summary>
    /// 自动模式
    /// </summary>
    Auto = 2
}
