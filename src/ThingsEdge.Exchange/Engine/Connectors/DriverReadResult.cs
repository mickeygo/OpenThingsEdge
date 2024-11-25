using ThingsEdge.Exchange.Contracts;

namespace ThingsEdge.Exchange.Engine.Connectors;

/// <summary>
/// 设备驱动读取数据结果。
/// </summary>
internal sealed class DriverReadResult : AbstractResult<PayloadData>
{
    /// <summary>
    /// 错误代码
    /// </summary>
    public int ErrorCode { get; set; }
}
