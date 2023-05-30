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
    /// 过站状态，1=>OK；2=>NG
    /// </summary>
    public int Pass { get; set; } = 1;

    /// <summary>
    /// 是否已存档。
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>
    /// CT 时长，单位秒。
    /// </summary>
    public int CycleTime { get; set; }

    /// <summary>
    /// 存档
    /// </summary>
    /// <param name="pass">过站状态，默认为 1（OK）</param>
    public void Archive(int pass = 1)
    {
        Pass = pass;
        IsArchived = true;
        ArchiveTime = DateTime.Now;
        CycleTime = Convert.ToInt32((ArchiveTime - EntryTime).Value.TotalSeconds);
    }

    /// <summary>
    /// 是否为 OK 过站
    /// </summary>
    /// <returns></returns>
    public bool IsOK() => Pass == 1;

    /// <summary>
    /// 是否为 NG 过站
    /// </summary>
    /// <returns></returns>
    public bool IsNG() => Pass == 2;
}
