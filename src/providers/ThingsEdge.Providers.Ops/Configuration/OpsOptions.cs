namespace ThingsEdge.Providers.Ops.Configuration;

/// <summary>
/// OPS 配置选项
/// </summary>
public sealed class OpsOptions
{
    /// <summary>
    /// 触发标记的触发条件值，值大于 0 才有效。
    /// </summary>
    public short TagTriggerConditionValue { get; set; }

    /// <summary>
    /// Heartbeat 心跳收到值后，是否要重置值并回写给设备，默认为 true。
    /// </summary>
    public bool HeartbeatShouldAckZero { get; set; } = true;

    /// <summary>
    /// 针对于S7协议，1500 系列一起读取运行的最多 PDU 长度（byte数量），为0时会使用默认长度。  
    /// </summary>
    public int Siemens_PDUSizeS1500 { get; set; }

    /// <summary>
    /// 针对于S7协议，1200 系列一起读取运行的最多 PDU 长度（byte数量），为0时会使用默认长度。  
    /// </summary>
    public int Siemens_PDUSizeS1200 { get; set; }

    /// <summary>
    /// 针对于S7协议，300 系列一起读取运行的最多 PDU 长度（byte数量），为0时会使用默认长度。  
    /// </summary>
    public int Siemens_PDUSizeS300 { get; set; }
}
