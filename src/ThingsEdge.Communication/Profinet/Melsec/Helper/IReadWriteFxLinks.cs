using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.Profinet.Melsec.Helper;

/// <summary>
/// 三菱的FxLink协议接口的设备信息
/// </summary>
public interface IReadWriteFxLinks : IReadWriteDevice, IReadWriteNet
{
    /// <summary>
    /// PLC的当前的站号，需要根据实际的值来设定，默认是0。
    /// </summary>
    byte Station { get; set; }

    /// <summary>
    /// 报文等待时间，单位10ms，设置范围为0-15。
    /// </summary>
    byte WaittingTime { get; set; }

    /// <summary>
    /// 是否启动和校验<br />
    /// Whether to start and sum verify
    /// </summary>
    bool SumCheck { get; set; }

    /// <summary>
    /// 当前的PLC的Fxlinks协议格式，通常是格式1，或是格式4，所以此处可以设置1，或者是4。
    /// </summary>
    int Format { get; set; }

    /// <summary>
    /// 启动PLC的操作，可以携带额外的参数信息，指定站号。举例：s=2; 注意：分号是必须的。
    /// </summary>
    /// <param name="parameter">允许携带的参数信息，例如s=2; 也可以为空</param>
    /// <returns>是否启动成功</returns>
    Task<OperateResult> StartPLCAsync(string parameter = "");

    /// <summary>
    /// 停止PLC的操作，可以携带额外的参数信息，指定站号。举例：s=2; 注意：分号是必须的。
    /// </summary>
    /// <param name="parameter">允许携带的参数信息，例如s=2; 也可以为空</param>
    /// <returns>是否停止成功</returns>
    Task<OperateResult> StopPLCAsync(string parameter = "");

    /// <summary>
    /// 读取PLC的型号信息，可以携带额外的参数信息，指定站号。举例：s=2; 注意：分号是必须的。
    /// </summary>
    /// <param name="parameter">允许携带的参数信息，例如s=2; 也可以为空</param>
    /// <returns>带PLC型号的结果信息</returns>
    Task<OperateResult<string>> ReadPlcTypeAsync(string parameter = "");
}
