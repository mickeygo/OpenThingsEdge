namespace ThingsEdge.Application.Domain.Entities;

/// <summary>
/// SN 过站数据。
/// </summary>
public sealed class SnTransitData : EntityBase
{
    /// <summary>
    /// SN
    /// </summary>
    [NotNull]
    public string? SN { get; set; }

    /// <summary>
    /// 站点
    /// </summary>
    [NotNull]
    public string? Station { get; set; }

    /// <summary>
    /// 过站状态（OK、NG）
    /// </summary>
    public SnStatusType Status { get; set; }

    /// <summary>
    /// 进站时间
    /// </summary>
    public DateTime EntryTime { get; set; }

    /// <summary>
    /// 出站时间
    /// </summary>
    public DateTime? ArchiveTime { get; set; }

    /// <summary>
    /// CT 时长，单位秒。
    /// </summary>
    public double CycleTime { get; set; }

    /// <summary>
    /// 出站
    /// </summary>
    public void Outbound()
    {
        ArchiveTime = DateTime.Now;
        CycleTime = (ArchiveTime - EntryTime).Value.TotalSeconds;
    }
}
