using System.Diagnostics;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.HslCommunication;
using ThingsEdge.Communication.ModBus;
using ThingsEdge.Communication.Profinet.Delta.Helper;

namespace ThingsEdge.Communication.Profinet.Delta;

/// <summary>
/// 台达PLC的串口转网口透传类，基于Modbus-Rtu协议开发，但是实际的通信管道使用的是网络，但是实际的地址是台达的地址进行读写操作。<br />
/// Delta PLC's serial port to network port transparent transmission class is developed based on the Modbus-Rtu protocol, 
/// but the actual communication channel uses the network, but the actual address is Delta's address for read and write operations.
/// </summary>
/// <remarks>
/// 适用于DVP-ES/EX/EC/SS型号，DVP-SA/SC/SX/EH型号以及AS300型号，地址参考API文档，同时地址可以携带站号信息，举例：[s=2;D100],[s=3;M100]，可以动态修改当前报文的站号信息。<br />
/// Suitable for DVP-ES/EX/EC/SS models, DVP-SA/SC/SX/EH models and AS300 model, the address refers to the API document, and the address can carry station number information,
/// for example: [s=2;D100],[s= 3;M100], you can dynamically modify the station number information of the current message.
/// </remarks>
public class DeltaSerialOverTcp : ModbusRtuOverTcp, IDelta, IReadWriteDevice, IReadWriteNet
{
    /// <inheritdoc cref="P:HslCommunication.Profinet.Delta.IDelta.Series" />
    public DeltaSeries Series { get; set; } = DeltaSeries.Dvp;


    /// <inheritdoc cref="M:HslCommunication.Profinet.Delta.DeltaSerial.#ctor" />
    public DeltaSerialOverTcp()
    {
        ByteTransform.DataFormat = DataFormat.CDAB;
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusTcpNet.#ctor(System.String,System.Int32,System.Byte)" />
    public DeltaSerialOverTcp(string ipAddress, int port = 502, byte station = 1)
        : base(ipAddress, port, station)
    {
        ByteTransform.DataFormat = DataFormat.CDAB;
    }

    /// <inheritdoc />
    public override OperateResult<string> TranslateToModbusAddress(string address, byte modbusCode)
    {
        return DeltaHelper.TranslateToModbusAddress(this, address, modbusCode);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Delta.DeltaTcpNet.ReadBool(System.String,System.UInt16)" />
    [HslMqttApi("ReadBoolArray", "")]
    public override OperateResult<bool[]> ReadBool(string address, ushort length)
    {
        return DeltaHelper.ReadBool(this, base.ReadBool, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Delta.DeltaTcpNet.Write(System.String,System.Boolean[])" />
    [HslMqttApi("WriteBoolArray", "")]
    public override OperateResult Write(string address, bool[] values)
    {
        return DeltaHelper.Write(this, base.Write, address, values);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Delta.DeltaTcpNet.Read(System.String,System.UInt16)" />
    [HslMqttApi("ReadByteArray", "")]
    public override OperateResult<byte[]> Read(string address, ushort length)
    {
        return DeltaHelper.Read(this, base.Read, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Delta.DeltaTcpNet.Write(System.String,System.Byte[])" />
    [HslMqttApi("WriteByteArray", "")]
    public override OperateResult Write(string address, byte[] value)
    {
        return DeltaHelper.Write(this, base.Write, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Delta.DeltaSerialOverTcp.ReadBool(System.String,System.UInt16)" />
    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return await DeltaHelper.ReadBoolAsync(this, [DebuggerHidden] (address, length) => base.ReadBoolAsync(address, length), address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Delta.DeltaSerialOverTcp.Write(System.String,System.Boolean[])" />
    public override async Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return await DeltaHelper.WriteAsync(this, [DebuggerHidden] (address, values) => base.WriteAsync(address, values), address, values);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Delta.DeltaSerialOverTcp.Read(System.String,System.UInt16)" />
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return await DeltaHelper.ReadAsync(this, [DebuggerHidden] (address, length) => base.ReadAsync(address, length), address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Delta.DeltaSerialOverTcp.Write(System.String,System.Byte[])" />
    public override async Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        return await DeltaHelper.WriteAsync(this, [DebuggerHidden] (address, value) => base.WriteAsync(address, value), address, value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"DeltaSerialOverTcp[{IpAddress}:{Port}]";
    }
}
