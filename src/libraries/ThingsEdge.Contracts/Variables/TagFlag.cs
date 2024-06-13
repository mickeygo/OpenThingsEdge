namespace ThingsEdge.Contracts.Variables;

/// <summary>
/// 标记标识。
/// </summary>
public enum TagFlag
{
    /// <summary>
    /// 常规标记。
    /// 此标记没有任何行为，会随 <see cref="Trigger"/>、<see cref="Notice"/> 或 <see cref="Switch"/> 的标识的触发而随着发送。
    /// </summary>
    [Description("常规")]
    Normal = 0,

    /// <summary>
    /// 该标记仅用于心跳。
    /// 该标记值改变时发送。
    /// </summary>
    /// <remarks>
    /// Heartbeat 会监听信号标识的变化，信号标识的数据类型必须为 <see cref="TagDataType.Bit"/>、<see cref="TagDataType.Byte"/>、<see cref="TagDataType.Word"/> 
    /// 或 <see cref="TagDataType.Int"/> 其中一种，只有在值为 true 或 1 时才会触发。
    /// </remarks>
    [Description("心跳")]
    Heartbeat = 1,

    /// <summary>
    /// 该标记用于触发数据发送。
    /// 该标记值改变时，只会触发一次数据发送行为。此行为会接收响应结果，同时响应状态会更新此标记值。
    /// </summary>
    /// <remarks>
    /// Trigger 会监听信号标识的变化，信号标识的数据类型必须 <see cref="TagDataType.Int"/> 或 <see cref="TagDataType.Byte"/>，
    /// 只有在值与设定的 TagTriggerConditionValue 值相同时才会触发，默认为 1。
    /// </remarks>
    [Description("触发")]
    Trigger = 2,

    /// <summary>
    /// 该标记用于轮询发生数据。
    /// 该标记数据不受跳变影响，会根据设置的扫描速率不间断发送。此行为不会接收响应结果。
    /// </summary>
    /// <remarks>若通知标识是在有数据变化时才推送时，其值应该是可比较的单一或数组类型，不能为浮点数。</remarks>
    [Description("通知")]
    Notice = 3,

    /// <summary>
    /// 该标记用于控制触发。
    /// 若该标记值是开启状态，其附属数据会随着设定的扫描速率不间断统一发送，可用于实时数据场景，如曲线数据。
    /// </summary>
    [Description("开关")]
    Switch = 4,
}
