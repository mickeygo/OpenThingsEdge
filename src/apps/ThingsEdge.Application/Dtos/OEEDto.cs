﻿namespace ThingsEdge.Application.Dtos;

public sealed class OEECollectionDto
{
    /// <summary>
    /// OEE 各设备
    /// </summary>
    public List<OEEDto> OeeList { get; init; } = new();

    /// <summary>
    /// 总性能效率
    /// </summary>
    public double TotalPerformanceRate { get; set; }
}

public sealed class OEEDto
{
    /// <summary>
    /// 设备
    /// </summary>
    public string? EquipmentCode { get; set; }

    /// <summary>
    /// 负荷时间
    /// </summary>
    public int LoadingTime { get; set; }

    /// <summary>
    /// 警报时间
    /// </summary>
    public int WarningTime { get; set; }

    /// <summary>
    /// 急停时间
    /// </summary>
    public int EStopingTime { get; set; }

    /// <summary>
    /// 加工时间
    /// </summary>
    public int WorkingTime { get; set; }

    /// <summary>
    /// 性能效率 (合格数+缺陷数)*单件标准耗时 / 实际工作时间
    /// </summary>
    public double PerformanceRate { get; set; }

    /// <summary>
    /// 加工 OK 数量
    /// </summary>
    public int OkCount { get; set; }

    /// <summary>
    /// 加工 NG 数量
    /// </summary>
    public int NgCount { get; set; }

    /// <summary>
    /// 加工总数量（OkCount + NgCount）
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 良率（一次性良品数 / 实际生产数）
    /// </summary>
    public double YieldRate { get; set; }
}