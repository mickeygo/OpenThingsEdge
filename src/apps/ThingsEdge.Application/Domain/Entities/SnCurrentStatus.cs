namespace ThingsEdge.Application.Domain.Entities;

/// <summary>
/// SN 当前状态
/// </summary>
[SugarTable("SnCurrentStatus")]
public sealed class SnCurrentStatus : EntityBase
{
    /// <summary>
    /// SN
    /// </summary>
    [NotNull]
    public string? SN { get; init; }

    /// <summary>
    /// 当前站点
    /// </summary>
    [NotNull]
    public string? Station { get; set; }

    /// <summary>
    /// 当前状态（OK、NG、返工）
    /// </summary>
    public SnStatusType Status { get; set; }
}
