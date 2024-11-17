using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.HslCommunication;
using ThingsEdge.Communication.ModBus;
using ThingsEdge.Communication.Profinet.Delta.Helper;

namespace ThingsEdge.Communication.Profinet.Delta;

/// <summary>
/// 台达PLC的串口通讯类，基于Modbus-Rtu协议开发，按照台达的地址进行实现。<br />
/// The serial communication class of Delta PLC is developed based on the Modbus-Rtu protocol and implemented according to Delta's address.
/// </summary>
/// <remarks>
/// 适用于DVP-ES/EX/EC/SS型号，DVP-SA/SC/SX/EH型号以及AS300型号，地址参考API文档，同时地址可以携带站号信息，举例：[s=2;D100],[s=3;M100]，可以动态修改当前报文的站号信息。<br />
/// Suitable for DVP-ES/EX/EC/SS models, DVP-SA/SC/SX/EH models and AS300 model, the address refers to the API document, and the address can carry station number information,
/// for example: [s=2;D100],[s= 3;M100], you can dynamically modify the station number information of the current message.
/// </remarks>
public class DeltaSerial : ModbusRtu, IDelta, IReadWriteDevice, IReadWriteNet
{
    /// <inheritdoc cref="P:HslCommunication.Profinet.Delta.IDelta.Series" />
    public DeltaSeries Series { get; set; } = DeltaSeries.Dvp;


    /// <summary>
    /// 实例化一个默认的对象<br />
    /// Instantiate a default object
    /// </summary>
    public DeltaSerial()
    {
        ByteTransform.DataFormat = DataFormat.CDAB;
    }

    /// <summary>
    /// 指定客户端自己的站号来初始化<br />
    /// Specify the client's own station number to initialize
    /// </summary>
    /// <param name="station">客户端自身的站号</param>
    public DeltaSerial(byte station = 1)
        : base(station)
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

    /// <inheritdoc />
    public override string ToString()
    {
        return $"DeltaSerial[{PortName}:{BaudRate}]";
    }
}
