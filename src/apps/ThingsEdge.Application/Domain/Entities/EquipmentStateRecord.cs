namespace ThingsEdge.Application.Domain.Entities;

/// <summary>
/// 设备状态记录。
/// </summary>
/// <remarks>
///  1. 记录设备每次开机时间和关机时间；
///  2. 记录设备做件的开始和结束时间；
/// </remarks>
public sealed class EquipmentStateRecord : Entity
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
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 时长，单位秒。
    /// </summary>
    public double Duration { get; set; }
}

public enum Equipment
{
    /// <summary>
    /// 运行。
    /// </summary>
    Running = 1,

    /// <summary>
    /// 工作。
    /// </summary>
    Working,

    Repair,
}

/// <summary>
/// 设备正处于的状态。
/// </summary>
public enum EquipmentState
{
    /// <summary>
    /// 空闲中。
    /// </summary>
    Idle = 1,

    /// <summary>
    /// 工作中。
    /// </summary>
    Working,

    /// <summary>
    /// 故障。
    /// </summary>
    Fault,

    /// <summary>
    /// 维修。
    /// </summary>
    Repair,

    /// <summary>
    /// 停机。
    /// </summary>
    Stop,
}
