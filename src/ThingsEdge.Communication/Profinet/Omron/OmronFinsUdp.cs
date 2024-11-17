using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.HslCommunication;
using ThingsEdge.Communication.Profinet.Omron.Helper;

namespace ThingsEdge.Communication.Profinet.Omron;

/// <summary>
/// 欧姆龙的Udp协议的实现类，地址类型和Fins-TCP一致，无连接的实现，可靠性不如<see cref="T:HslCommunication.Profinet.Omron.OmronFinsNet" /><br />
/// Omron's Udp protocol implementation class, the address type is the same as Fins-TCP, 
/// and the connectionless implementation is not as reliable as <see cref="T:HslCommunication.Profinet.Omron.OmronFinsNet" />
/// </summary>
/// <remarks>
/// <inheritdoc cref="T:HslCommunication.Profinet.Omron.OmronFinsNet" path="remarks" />
/// </remarks>
public class OmronFinsUdp : DeviceUdpNet, IOmronFins, IReadWriteDevice, IReadWriteNet
{
    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.ICF" />
    public byte ICF { get; set; } = 128;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.RSV" />
    public byte RSV { get; private set; } = 0;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.GCT" />
    public byte GCT { get; set; } = 2;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.DNA" />
    public byte DNA { get; set; } = 0;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.DA1" />
    public byte DA1 { get; set; } = 0;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.DA2" />
    public byte DA2 { get; set; } = 0;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.SNA" />
    public byte SNA { get; set; } = 0;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.SA1" />
    public byte SA1 { get; set; } = 13;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.SA2" />
    public byte SA2 { get; set; }

    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.SID" />
    public byte SID { get; set; } = 0;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.ReadSplits" />
    public int ReadSplits { get; set; } = 500;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.Helper.IOmronFins.PlcType" />
    public OmronPlcType PlcType { get; set; } = OmronPlcType.CSCJ;


    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.#ctor(System.String,System.Int32)" />
    public OmronFinsUdp(string ipAddress, int port)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.#ctor" />
    public OmronFinsUdp()
    {
        WordLength = 1;
        ByteTransform = new RegularByteTransform(DataFormat.CDAB);
        ByteTransform.IsStringReverseByteWord = true;
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new FinsUdpMessage();
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.PackCommand(System.Byte[])" />
    private byte[] PackCommand(byte[] cmd)
    {
        var array = new byte[10 + cmd.Length];
        array[0] = ICF;
        array[1] = RSV;
        array[2] = GCT;
        array[3] = DNA;
        array[4] = DA1;
        array[5] = DA2;
        array[6] = SNA;
        array[7] = SA1;
        array[8] = SA2;
        array[9] = SID;
        cmd.CopyTo(array, 10);
        return array;
    }

    /// <inheritdoc />
    public override byte[] PackCommandWithHeader(byte[] command)
    {
        return PackCommand(command);
    }

    /// <inheritdoc />
    public override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        return OmronFinsNetHelper.UdpResponseValidAnalysis(response);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.Read(System.String,System.UInt16)" />
    [HslMqttApi("ReadByteArray", "")]
    public override OperateResult<byte[]> Read(string address, ushort length)
    {
        return OmronFinsNetHelper.Read(this, address, length, ReadSplits);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.Write(System.String,System.Byte[])" />
    [HslMqttApi("WriteByteArray", "")]
    public override OperateResult Write(string address, byte[] value)
    {
        return OmronFinsNetHelper.Write(this, address, value);
    }

    /// <inheritdoc />
    [HslMqttApi("ReadString", "")]
    public override OperateResult<string> ReadString(string address, ushort length)
    {
        return base.ReadString(address, length, Encoding.UTF8);
    }

    /// <inheritdoc />
    [HslMqttApi("WriteString", "")]
    public override OperateResult Write(string address, string value)
    {
        return base.Write(address, value, Encoding.UTF8);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.Read(HslCommunication.Profinet.Omron.Helper.IOmronFins,System.String[])" />
    public OperateResult<byte[]> Read(string[] address)
    {
        return OmronFinsNetHelper.Read(this, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.ReadBool(System.String,System.UInt16)" />
    [HslMqttApi("ReadBoolArray", "")]
    public override OperateResult<bool[]> ReadBool(string address, ushort length)
    {
        return OmronFinsNetHelper.ReadBool(this, address, length, ReadSplits);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.Write(System.String,System.Boolean[])" />
    [HslMqttApi("WriteBoolArray", "")]
    public override OperateResult Write(string address, bool[] values)
    {
        return OmronFinsNetHelper.Write(this, address, values);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.Run(HslCommunication.Core.IReadWriteDevice)" />
    [HslMqttApi(ApiTopic = "Run", Description = "将CPU单元的操作模式更改为RUN，从而使PLC能够执行其程序。")]
    public OperateResult Run()
    {
        return OmronFinsNetHelper.Run(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.Stop(HslCommunication.Core.IReadWriteDevice)" />
    [HslMqttApi(ApiTopic = "Stop", Description = "将CPU单元的操作模式更改为PROGRAM，停止程序执行。")]
    public OperateResult Stop()
    {
        return OmronFinsNetHelper.Stop(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.ReadCpuUnitData(HslCommunication.Core.IReadWriteDevice)" />
    [HslMqttApi(ApiTopic = "ReadCpuUnitData", Description = "读取CPU的一些数据信息，主要包含型号，版本，一些数据块的大小。")]
    public OperateResult<OmronCpuUnitData> ReadCpuUnitData()
    {
        return OmronFinsNetHelper.ReadCpuUnitData(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.ReadCpuUnitStatus(HslCommunication.Core.IReadWriteDevice)" />
    [HslMqttApi(ApiTopic = "ReadCpuUnitStatus", Description = "读取CPU单元的一些操作状态数据，主要包含运行状态，工作模式，错误信息等。")]
    public OperateResult<OmronCpuUnitStatus> ReadCpuUnitStatus()
    {
        return OmronFinsNetHelper.ReadCpuUnitStatus(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.ReadCpuTime(HslCommunication.Core.IReadWriteDevice)" />
    [HslMqttApi(ApiTopic = "ReadCpuTime", Description = "读取CPU单元的时间信息。")]
    public OperateResult<DateTime> ReadCpuTime()
    {
        return OmronFinsNetHelper.ReadCpuTime(this);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"OmronFinsUdp[{IpAddress}:{Port}]";
    }
}
