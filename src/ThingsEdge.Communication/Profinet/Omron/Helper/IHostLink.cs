using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.Profinet.Omron.Helper;

/// <summary>
/// HostLink的接口实现。
/// </summary>
public interface IHostLink : IReadWriteDevice, IReadWriteNet
{
    /// <summary>
    /// 信息控制字段。
    /// </summary>
    byte ICF { get; set; }

    /// <summary>
    /// PLC的单元号地址，通常都为0。
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///   <item>00: CPU Unit</item>
    ///   <item>FE: Controller Link Unit or Ethernet Unit connected to network</item>
    ///   <item>10 TO 1F: CPU Bus Unit</item>
    ///   <item>E1: Inner Board</item>
    /// </list>
    /// </remarks>
    byte DA2 { get; set; }

    /// <summary>
    /// 上位机的单元号地址。
    /// </summary>
    /// <remarks>
    /// 00: CPU Unit<br />
    /// 10-1F: CPU Bus Unit
    /// </remarks>
    byte SA2 { get; set; }

    /// <summary>
    /// 服务的标识号，由客户端生成自增的顺序值，用来标识和校验通信报文的ID。
    /// </summary>
    byte SID { get; set; }

    /// <summary>
    /// The response wait time sets the time from when the CPU Unit receives a command block until it starts 
    /// to return a response.It can be set from 0 to F in hexadecimal, in units of 10 ms.
    /// </summary>
    byte ResponseWaitTime { get; set; }

    /// <summary>
    /// PLC设备的站号信息
    /// </summary>
    byte UnitNumber { get; set; }

    /// <summary>
    /// 进行字读取的时候对于超长的情况按照本属性进行切割。
    /// </summary>
    int ReadSplits { get; set; }

    OmronPlcType PlcType { get; set; }
}
