using System.ComponentModel;

namespace ThingsEdge.Communication;

/// <summary>
/// 通信库错误代码，错误代码区间为 [900, 999]。
/// </summary>
public enum CommErrorCode
{
    [Description("错误")]
    Error = 900,

    /// <summary>
    /// 数据地址解析异常
    /// </summary>
    [Description("数据地址解析异常")]
    DataAddressParseError = 901,

    /// <summary>
    /// 构建发送命令数据包异常
    /// </summary>
    [Description("构建发送命令数据包异常")]
    BuildSendCommandError,
}
