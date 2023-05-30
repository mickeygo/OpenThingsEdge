namespace ThingsEdge.Application.Dtos;

/// <summary>
/// OEE 查询
/// </summary>
public sealed class OEEQueryInput
{
    public string? Line { get; set; }

    /// <summary>
    /// 查询范围开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 查询范围结束时间
    /// </summary>
    public DateTime EndTime { get; set; }
}
