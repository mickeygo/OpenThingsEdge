namespace ThingsEdge.Providers.Ops.Configuration;

/// <summary>
/// 内部全局设定。
/// </summary>
internal static class InternalGlobalSetting
{
    /// <summary>
    /// 触发标记的触发条件值，默认为 1。
    /// </summary>
    internal static short TagTriggerConditionValue { get; set; } = 1;

    /// <summary>
    /// Siemens 选项设置
    /// </summary>
    internal static class SiemensS7NetOption
    {
        /// <summary>
        /// 针对于S7协议，1500 系列一起读取运行的最多 PDU 长度（byte数量），为0时会使用从CPU中读取的 PDU 长度。  
        /// </summary>
        public static int S1500_PDUSize = 462;

        /// <summary>
        /// 针对于S7协议，1200 系列一起读取运行的最多 PDU 长度（byte数量），为0时会使用从CPU中读取的 PDU 长度。  
        /// </summary>
        public static int S1200_PDUSize = 462;

        /// <summary>
        /// 针对于S7协议，300 系列一起读取运行的最多 PDU 长度（byte数量），为0时会使用从CPU中读取的 PDU 长度。  
        /// </summary>
        public static int S300_PDUSize = 32;
    }
}
