namespace ThingsEdge.Application.Domain.Entities;

/// <summary>
/// SN 过站记录日志。
/// </summary>
[SugarTable("sn_transit_record_log")]
public sealed class SnTransitRecordLog : EntityBaseId
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
    /// 过站类型，1=>进站; 2=>出站
    /// </summary>
    public int TransitType { get; init; }

    /// <summary>
    /// 记录时间
    /// </summary>
    public DateTime RecordTime { get; init; }
}
