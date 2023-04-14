namespace ThingsEdge.Contracts.Devices;

/// <summary>
/// 设备驱动型号，如西门子1200、西门子1500、三菱、欧姆龙FinsTcp、AB CIP等。
/// </summary>
public enum DriverModel
{
    ModbusTcp = 1,

    /// <summary>
    /// 支持西门子 1500 系列。
    /// </summary>
    S7_1500,

    /// <summary>
    /// 支持西门子 1200 系列。
    /// </summary>
    S7_1200,

    S7_400,

    S7_300,

    S7_S200,

    S7_S200Smart,

    Melsec_A1E,

    Melsec_CIP,

    /// <summary>
    /// 支持 Q、Qna 系列。
    /// </summary>
    Melsec_MC,

    /// <summary>
    /// 支持三菱 R 系列。
    /// </summary>
    Melsec_MCR,

    /// <summary>
    /// 支持三菱 CS、CP、CJ 系列。
    /// </summary>
    Omron_FinsTcp,

    Omron_CipNet,

    Omron_HostLinkOverTcp,

    Omron_HostLinkCModeOverTcp,

    AllenBradley_CIP,
}
