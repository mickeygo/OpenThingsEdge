namespace ThingsEdge.Exchange.Contracts.Variables;

/// <summary>
/// 设备驱动型号，如西门子1200、西门子1500、三菱、欧姆龙FinsTcp、AB CIP等。
/// </summary>
public enum DriverModel
{
    /// <summary>
    /// ModbusTcp 协议。
    /// </summary>
    ModbusTcp = 1,

    /// <summary>
    /// 西门子 S7 协议，支持 1500 系列。
    /// </summary>
    S7_1500,

    /// <summary>
    /// 西门子 S7 协议，支持 1200 系列。
    /// </summary>
    S7_1200,

    /// <summary> 
    /// 西门子 S7 协议，支持 400 系列。
    /// </summary>
    S7_400,

    /// <summary>
    /// 西门子 S7 协议，支持 300 系列。
    /// </summary>
    S7_300,

    /// <summary>
    /// 西门子 S7 协议，支持 S200 系列。
    /// </summary>
    S7_S200,

    /// <summary>
    /// 西门子 S7 协议，支持 S200Smart 系列。
    /// </summary>
    S7_S200Smart,

    /// <summary>
    /// 三菱 MC 协议，支持 Q、Qna 系列。
    /// </summary>
    /// <remarks>二进制码通讯。</remarks>
    Melsec_MC,

    /// <summary>
    /// 三菱 MC 协议，支持 Q、Qna 系列。
    /// </summary>
    /// <remarks>ASCII码通信。</remarks>
    Melsec_MCAscii,

    /// <summary>
    /// 三菱 MC 协议，支持 R 系列。
    /// </summary>
    Melsec_MCR,

    /// <summary>
    /// 欧姆龙系列。
    /// </summary>
    Omron_FinsTcp,

    /// <summary>
    /// 欧姆龙 Fins-Tcp 通信协议，支持 NJ、NX、NY 系列
    /// </summary>
    Omron_Cip,

    /// <summary>
    /// 罗克韦尔 CIP 协议，支持  1756，1769 等型号
    /// </summary>
    AllenBradley_CIP,

    /// <summary>
    /// 汇川的网口通信协议，支持 AM400、AM400_800、AC800、H3U、XP、H5U 等系列
    /// </summary>
    Inovance_Tcp,

    /// <summary>
    /// 台达PLC的网口通讯类，支持 DVP-ES/EX/EC/SS型号，DVP-SA/SC/SX/EH 型号以及 AS300 型号
    /// </summary>
    Delta_Tcp,

    /// <summary>
    /// 富士PLC的 SPH 通信协议
    /// </summary>
    Fuji_SPH,

    /// <summary>
    /// 松下PLC，基于MC协议的实现。
    /// </summary>
    Panasonic_Mc,

    /// <summary>
    /// 信捷PLC，支持 XC、XD、XL 系列
    /// </summary>
    XinJE_Tcp,
}
