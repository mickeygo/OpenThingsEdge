namespace ThingsEdge.Contracts;

/// <summary>
/// 标记标识。
/// </summary>
public enum TagFlag
{
    /// <summary>
    /// 常规地址。
    /// 只会随 <see cref="Trigger"/> 或 <see cref="Switch"/> 的触发而随着发送。
    /// </summary>
    [Description("常规")]
    Normal = 0,

    /// <summary>
    /// 表示该地址用于心跳。
    /// </summary>
    [Description("心跳")]
    Heartbeat = 1,

    /// <summary>
    /// 表示该地址触发数据发送。
    /// 该地址值改变时，会触发数据发送行为。
    /// </summary>
    [Description("触发")]
    Trigger = 2,

    /// <summary>
    /// 表示该地址数据会根据设置的扫描速率不间断发送，如警报。
    /// </summary>
    [Description("通知")]
    Notice = 3,

    /// <summary>
    /// 若该地址值是开启状态，其附属数据会实时不间断发送，主要用于实时数据场景，如曲线数据。
    /// </summary>
    [Description("开关")]
    Switch = 4,

    /// <summary>
    /// API 底层触发，类似 Trigger，但不会受跳变影响发送数据。
    /// </summary>
    [Description("底层")]
    Underly = 9,
}
