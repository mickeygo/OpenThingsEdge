using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Profinet.Melsec.Helper;

/// <summary>
/// 三菱的FxLink协议接口的设备信息
/// </summary>
public interface IReadWriteFxLinks : IReadWriteDevice, IReadWriteNet
{
    /// <summary>
    /// PLC的当前的站号，需要根据实际的值来设定，默认是0<br />
    /// The current station number of the PLC needs to be set according to the actual value. The default is 0.
    /// </summary>
    byte Station { get; set; }

    /// <summary>
    /// 报文等待时间，单位10ms，设置范围为0-15<br />
    /// Message waiting time, unit is 10ms, setting range is 0-15
    /// </summary>
    byte WaittingTime { get; set; }

    /// <summary>
    /// 是否启动和校验<br />
    /// Whether to start and sum verify
    /// </summary>
    bool SumCheck { get; set; }

    /// <summary>
    /// 当前的PLC的Fxlinks协议格式，通常是格式1，或是格式4，所以此处可以设置1，或者是4<br />
    /// The current PLC Fxlinks protocol format is usually format 1 or format 4, so it can be set to 1 or 4 here
    /// </summary>
    int Format { get; set; }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecFxLinksOverTcp.StartPLC(System.String)" />
    OperateResult StartPLC(string parameter = "");

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecFxLinksOverTcp.StopPLC(System.String)" />
    OperateResult StopPLC(string parameter = "");

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecFxLinksOverTcp.ReadPlcType(System.String)" />
    OperateResult<string> ReadPlcType(string parameter = "");
}
