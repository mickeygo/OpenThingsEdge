namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// Siemens 读取数据结果。
/// </summary>
public sealed class SiemensResult : AbstractResult<PayloadData>
{
    /// <summary>
    /// 错误代码
    /// </summary>
    public int ErrorCode { get; set; }
}

/// <summary>
/// Siemens 读取数据结果，结果为数据集。
/// </summary>
public sealed class SiemensResult2 : AbstractResult<List<PayloadData>>
{
    /// <summary>
    /// 错误代码
    /// </summary>
    public int ErrorCode { get; set; }
}