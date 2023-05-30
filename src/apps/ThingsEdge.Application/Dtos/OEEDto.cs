namespace ThingsEdge.Application.Dtos;

public sealed class OEEDto
{
    /// <summary>
    /// 设备
    /// </summary>
    public string? EquipmentCode { get; set; }

    /// <summary>
    /// 运行时长（开机时长）
    /// </summary>
    public int RunningDuration { get; set; }

    /// <summary>
    /// 负荷时长
    /// </summary>
    public int LoadingDuration { get; set; }

    /// <summary>
    /// 设备利用率
    /// </summary>
    public double Rate { get; set; }

    /// <summary>
    /// 产能利用率
    /// </summary>
    public double Rate2 { get; set; }
}
