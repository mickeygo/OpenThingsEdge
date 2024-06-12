namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// 设备驱动读取数据结果。
/// </summary>
public sealed class DriverReadResult : AbstractResult<PayloadData>
{
    /// <summary>
    /// 错误代码
    /// </summary>
    public int ErrorCode { get; set; }
}
