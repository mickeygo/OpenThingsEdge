using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.HslCommunication;
using ThingsEdge.Communication.Profinet.Omron.Helper;

namespace ThingsEdge.Communication.Profinet.Omron;

/// <summary>
/// 欧姆龙的HostLink的C-Mode实现形式，当前的类是通过以太网透传实现。地址支持携带站号信息，例如：s=2;D100<br />
/// The C-Mode implementation form of Omron’s HostLink, the current class is realized through Ethernet transparent transmission. 
/// Address supports carrying station number information, for example: s=2;D100
/// </summary>
/// <remarks>
/// 暂时只支持的字数据的读写操作，不支持位的读写操作。另外本模式下，程序要在监视模式运行才能写数据，欧姆龙官方回复的。
/// </remarks>
public sealed class OmronHostLinkCModeOverTcp : DeviceTcpNet, IHostLinkCMode, IReadWriteNet
{
    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronHostLinkOverTcp.UnitNumber" />
    public byte UnitNumber { get; set; }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.#ctor" />
    public OmronHostLinkCModeOverTcp()
    {
        ByteTransform = new RegularByteTransform(DataFormat.CDAB);
        WordLength = 1;
        ByteTransform.IsStringReverseByteWord = true;
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronCipNet.#ctor(System.String,System.Int32)" />
    public OmronHostLinkCModeOverTcp(string ipAddress, int port)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new SpecifiedCharacterMessage(13);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronHostLinkCModeOverTcp.Read(System.String,System.UInt16)" />
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return await OmronHostLinkCModeHelper.ReadAsync(this, UnitNumber, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronHostLinkCModeOverTcp.Write(System.String,System.Byte[])" />
    public override async Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        return await OmronHostLinkCModeHelper.WriteAsync(this, UnitNumber, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ReadPlcType(HslCommunication.Core.IReadWriteDevice,System.Byte)" />
    [HslMqttApi("读取PLC的当前的型号信息")]
    public OperateResult<string> ReadPlcType()
    {
        return ReadPlcType(UnitNumber);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ReadPlcType(HslCommunication.Core.IReadWriteDevice,System.Byte)" />
    public OperateResult<string> ReadPlcType(byte unitNumber)
    {
        return OmronHostLinkCModeHelper.ReadPlcType(this, unitNumber);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ReadPlcMode(HslCommunication.Core.IReadWriteDevice,System.Byte)" />
    [HslMqttApi("读取PLC当前的操作模式，0: 编程模式  1: 运行模式  2: 监视模式")]
    public OperateResult<int> ReadPlcMode()
    {
        return ReadPlcMode(UnitNumber);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ReadPlcMode(HslCommunication.Core.IReadWriteDevice,System.Byte)" />
    public OperateResult<int> ReadPlcMode(byte unitNumber)
    {
        return OmronHostLinkCModeHelper.ReadPlcMode(this, unitNumber);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ChangePlcMode(HslCommunication.Core.IReadWriteDevice,System.Byte,System.Byte)" />
    [HslMqttApi("将当前PLC的模式变更为指定的模式，0: 编程模式  1: 运行模式  2: 监视模式")]
    public OperateResult ChangePlcMode(byte mode)
    {
        return ChangePlcMode(UnitNumber, mode);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ChangePlcMode(HslCommunication.Core.IReadWriteDevice,System.Byte,System.Byte)" />
    public OperateResult ChangePlcMode(byte unitNumber, byte mode)
    {
        return OmronHostLinkCModeHelper.ChangePlcMode(this, unitNumber, mode);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"OmronHostLinkCModeOverTcp[{IpAddress}:{Port}]";
    }
}
