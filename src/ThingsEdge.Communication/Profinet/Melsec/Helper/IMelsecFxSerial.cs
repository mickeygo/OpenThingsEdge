using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.Profinet.Melsec.Helper;

/// <summary>
/// 三菱的串口的接口类对象
/// </summary>
public interface IMelsecFxSerial : IReadWriteNet
{
    /// <summary>
    /// 激活PLC的接收状态。
    /// </summary>
    /// <returns>是否激活成功</returns>
    Task<OperateResult> ActivePlcAsync();
}
