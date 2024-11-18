using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.Profinet.Omron.Helper;

/// <summary>
/// Fins协议的接口对象
/// </summary>
public interface IOmronFins : IReadWriteDevice, IReadWriteNet
{
    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.ICF" />
    byte ICF { get; set; }

    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.RSV" />
    byte RSV { get; }

    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.GCT" />
    byte GCT { get; set; }

    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.DNA" />
    byte DNA { get; set; }

    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.DA1" />
    byte DA1 { get; set; }

    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.DA2" />
    byte DA2 { get; set; }

    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.SNA" />
    byte SNA { get; set; }

    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.SA1" />
    byte SA1 { get; set; }

    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.SA2" />
    byte SA2 { get; set; }

    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.SID" />
    byte SID { get; set; }

    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.ReadSplits" />
    int ReadSplits { get; set; }

    /// <summary>
    /// 获取或设置欧姆龙PLC的系列信息<br />
    /// Obtain or set the series information of Omron PLC
    /// </summary>
    OmronPlcType PlcType { get; set; }

    Task<OperateResult> RunAsync();

    Task<OperateResult> StopAsync();

    Task<OperateResult<OmronCpuUnitData>> ReadCpuUnitDataAsync();

    Task<OperateResult<OmronCpuUnitStatus>> ReadCpuUnitStatusAsync();

    Task<OperateResult<DateTime>> ReadCpuTimeAsync();
}
