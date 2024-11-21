using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.Profinet.Omron.Helper;

/// <summary>
/// Fins协议的接口对象
/// </summary>
public interface IOmronFins : IReadWriteDevice, IReadWriteNet
{
    /// <summary>
    /// 信息控制字段。
    /// </summary>
    byte ICF { get; set; }

    byte RSV { get; }

    byte GCT { get; set; }

    byte DNA { get; set; }

    byte DA1 { get; set; }

    byte DA2 { get; set; }

    byte SNA { get; set; }

    byte SA1 { get; set; }

    byte SA2 { get; set; }

    byte SID { get; set; }

    int ReadSplits { get; set; }

    /// <summary>
    /// 获取或设置欧姆龙PLC的系列信息。
    /// </summary>
    OmronPlcType PlcType { get; set; }

    Task<OperateResult> RunAsync();

    Task<OperateResult> StopAsync();

    Task<OperateResult<OmronCpuUnitData>> ReadCpuUnitDataAsync();

    Task<OperateResult<OmronCpuUnitStatus>> ReadCpuUnitStatusAsync();

    Task<OperateResult<DateTime>> ReadCpuTimeAsync();
}
