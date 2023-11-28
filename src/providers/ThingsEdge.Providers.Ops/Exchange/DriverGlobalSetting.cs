namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// 驱动全局设定。
/// </summary>
public static class DriverGlobalSetting
{
    public static class SiemensS7NetOption
    {
        /// <summary>
        /// 针对于S7协议，1500 系列一起读取运行的最多 PDU 长度（byte数量），为0时会使用从CPU中读取的 PDU 长度。  
        /// </summary>
        public static int S1500_PDULength = 462;

        /// <summary>
        /// 针对于S7协议，1200 系列一起读取运行的最多 PDU 长度（byte数量），为0时会使用从CPU中读取的 PDU 长度。  
        /// </summary>
        public static int S1200_PDULength = 462;

        /// <summary>
        /// 针对于S7协议，300 系列一起读取运行的最多 PDU 长度（byte数量），为0时会使用从CPU中读取的 PDU 长度。  
        /// </summary>
        public static int S300_PDULength = 32;
    }
}
