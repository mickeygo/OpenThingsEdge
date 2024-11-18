using System.IO.Ports;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Profinet.Omron.Helper;

namespace ThingsEdge.Communication.Profinet.Omron;

/// <summary>
/// 欧姆龙的HostLink协议的实现，地址支持示例 DM区:D100; CIO区:C100; Work区:W100; Holding区:H100; Auxiliary区: A100<br />
/// Implementation of Omron's HostLink protocol, address support example DM area: D100; CIO area: C100; Work area: W100; Holding area: H100; Auxiliary area: A100
/// </summary>
/// <remarks>
/// 感谢 深圳～拾忆 的测试，地址可以携带站号信息，例如 s=2;D100 
/// <br />
/// <note type="important">
/// 如果发现串口线和usb同时打开才能通信的情况，需要按照如下的操作：<br />
/// 串口线不是标准的串口线，电脑的串口线的235引脚分别接PLC的329引脚，45线短接，就可以通讯，感谢 深圳-小君(QQ932507362)提供的解决方案。
/// </note>
/// </remarks>
/// <example>
/// <inheritdoc cref="T:HslCommunication.Profinet.Omron.OmronHostLinkOverTcp" path="example" />
/// </example>
public class OmronHostLink : DeviceSerialPort, IHostLink, IReadWriteDevice, IReadWriteNet
{
    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronHostLinkOverTcp.ICF" />
    public byte ICF { get; set; } = 0;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronHostLinkOverTcp.DA2" />
    public byte DA2 { get; set; } = 0;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronHostLinkOverTcp.SA2" />
    public byte SA2 { get; set; }

    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronHostLinkOverTcp.SID" />
    public byte SID { get; set; } = 0;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronHostLinkOverTcp.ResponseWaitTime" />
    public byte ResponseWaitTime { get; set; } = 48;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronHostLinkOverTcp.UnitNumber" />
    public byte UnitNumber { get; set; }

    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronHostLinkOverTcp.ReadSplits" />
    public int ReadSplits { get; set; } = 260;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.Helper.IOmronFins.PlcType" />
    public OmronPlcType PlcType { get; set; } = OmronPlcType.CSCJ;


    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.#ctor" />
    public OmronHostLink()
    {
        ByteTransform = new RegularByteTransform(DataFormat.CDAB);
        WordLength = 1;
        ByteTransform.IsStringReverseByteWord = true;
        ReceiveEmptyDataCount = 5;
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new SpecifiedCharacterMessage(13);
    }

    /// <inheritdoc />
    public override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        return OmronHostLinkHelper.ResponseValidAnalysis(send, response);
    }

    /// <summary>
    /// 初始化串口信息，9600波特率，7位数据位，1位停止位，偶校验<br />
    /// Initial serial port information, 9600 baud rate, 7 data bits, 1 stop bit, even parity
    /// </summary>
    /// <param name="portName">端口号信息，例如"COM3"</param>
    public override void SerialPortInni(string portName)
    {
        base.SerialPortInni(portName);
    }

    /// <summary>
    /// 初始化串口信息，波特率，7位数据位，1位停止位，偶校验<br />
    /// Initializes serial port information, baud rate, 7-bit data bit, 1-bit stop bit, even parity
    /// </summary>
    /// <param name="portName">端口号信息，例如"COM3"</param>
    /// <param name="baudRate">波特率</param>
    public override void SerialPortInni(string portName, int baudRate)
    {
        base.SerialPortInni(portName, baudRate, 7, StopBits.One, Parity.Even);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.Read(System.String,System.UInt16)" />
    [HslMqttApi("ReadByteArray", "")]
    public override OperateResult<byte[]> Read(string address, ushort length)
    {
        return OmronHostLinkHelper.Read(this, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.Write(System.String,System.Byte[])" />
    [HslMqttApi("WriteByteArray", "")]
    public override OperateResult Write(string address, byte[] value)
    {
        return OmronHostLinkHelper.Write(this, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.Helper.OmronHostLinkHelper.Read(HslCommunication.Profinet.Omron.Helper.IHostLink,System.String[])" />
    public OperateResult<byte[]> Read(string[] address)
    {
        return OmronHostLinkHelper.Read(this, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.ReadBool(System.String,System.UInt16)" />
    [HslMqttApi("ReadBoolArray", "")]
    public override OperateResult<bool[]> ReadBool(string address, ushort length)
    {
        return OmronHostLinkHelper.ReadBool(this, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.Write(System.String,System.Boolean[])" />
    [HslMqttApi("WriteBoolArray", "")]
    public override OperateResult Write(string address, bool[] values)
    {
        return OmronHostLinkHelper.Write(this, address, values);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"OmronHostLink[{PortName}:{BaudRate}]";
    }
}
