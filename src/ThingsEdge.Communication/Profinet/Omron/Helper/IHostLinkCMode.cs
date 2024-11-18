using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.Profinet.Omron.Helper;

/// <summary>
/// HostLinkCMode协议的接口信息
/// </summary>
public interface IHostLinkCMode : IReadWriteNet
{
    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ReadPlcType(HslCommunication.Core.IReadWriteDevice,System.Byte)" />
    OperateResult<string> ReadPlcType();

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ReadPlcType(HslCommunication.Core.IReadWriteDevice,System.Byte)" />
    OperateResult<string> ReadPlcType(byte unitNumber);

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ReadPlcMode(HslCommunication.Core.IReadWriteDevice,System.Byte)" />
    OperateResult<int> ReadPlcMode();

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ReadPlcMode(HslCommunication.Core.IReadWriteDevice,System.Byte)" />
    OperateResult<int> ReadPlcMode(byte unitNumber);

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ChangePlcMode(HslCommunication.Core.IReadWriteDevice,System.Byte,System.Byte)" />
    OperateResult ChangePlcMode(byte mode);

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ChangePlcMode(HslCommunication.Core.IReadWriteDevice,System.Byte,System.Byte)" />
    OperateResult ChangePlcMode(byte unitNumber, byte mode);
}
