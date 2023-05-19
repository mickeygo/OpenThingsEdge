namespace ThingsEdge.Application.Domain.Entities;

/// <summary>
/// SN 过站记录。
/// </summary>
[SugarTable("sn_transit_record")]
internal sealed class SnTransitRecord : EntityBaseId
{
    /// <summary>
    /// SN
    /// </summary>
    [NotNull]
    public string? SN { get; init; }

    /// <summary>
    /// 产线
    /// </summary>
    [NotNull]
    public string? Line { get; init; }

    /// <summary>
    /// 工站
    /// </summary>
    [NotNull]
    public string? Station { get; init; }

    /// <summary>
    /// 进站时间
    /// </summary>
    public DateTime EntryTime { get; set; }

    /// <summary>
    /// 出站时间
    /// </summary>
    public DateTime? ArchiveTime { get; set; }

    /// <summary>
    /// 是否已存档。
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>
    /// CT 时长，单位秒。
    /// </summary>
    public int CycleTime { get; set; }

    /// <summary>
    /// 过站
    /// </summary>
    public void Outbound()
    {
        IsArchived = true;
        ArchiveTime = DateTime.Now;
        CycleTime = Convert.ToInt32((ArchiveTime - EntryTime).Value.TotalSeconds);
    }
}
