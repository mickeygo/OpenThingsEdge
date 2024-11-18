using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.Profinet.Melsec.Helper;

/// <summary>
/// 三菱的串口的接口类对象
/// </summary>
public interface IMelsecFxSerial : IReadWriteNet
{
    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.MelsecFxSerialHelper.ActivePlc(HslCommunication.Core.IReadWriteDevice)" />
    OperateResult ActivePlc();

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.MelsecFxSerialHelper.ActivePlc(HslCommunication.Core.IReadWriteDevice)" />
    Task<OperateResult> ActivePlcAsync();
}
