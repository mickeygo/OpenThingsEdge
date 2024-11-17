using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Profinet.Siemens.Helper;

/// <summary>
/// 西门子PPI的公用接口信息
/// </summary>
public interface ISiemensPPI : IReadWriteNet
{
    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.Helper.SiemensPPIHelper.Start(HslCommunication.Core.IReadWriteDevice,System.String,System.Byte,System.Object)" />
    OperateResult Start(string parameter = "");

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.Helper.SiemensPPIHelper.Stop(HslCommunication.Core.IReadWriteDevice,System.String,System.Byte,System.Object)" />
    OperateResult Stop(string parameter = "");

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.Helper.SiemensPPIHelper.ReadPlcType(HslCommunication.Core.IReadWriteDevice,System.String,System.Byte,System.Object)" />
    OperateResult<string> ReadPlcType(string parameter = "");
}
