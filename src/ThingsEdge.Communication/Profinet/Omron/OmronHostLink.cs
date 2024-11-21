using System.IO.Ports;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Profinet.Omron.Helper;

namespace ThingsEdge.Communication.Profinet.Omron;

/// <summary>
/// 欧姆龙的HostLink协议的实现，地址支持示例 DM区:D100; CIO区:C100; Work区:W100; Holding区:H100; Auxiliary区: A100。
/// </summary>
public class OmronHostLink : DeviceSerialPort, IHostLink, IReadWriteDevice, IReadWriteNet
{
    public byte ICF { get; set; }

    public byte DA2 { get; set; }

    public byte SA2 { get; set; }

    public byte SID { get; set; }

    public byte ResponseWaitTime { get; set; } = 48;

    public byte UnitNumber { get; set; }

    public int ReadSplits { get; set; } = 260;

    public OmronPlcType PlcType { get; set; } = OmronPlcType.CSCJ;

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
    protected override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        return OmronHostLinkHelper.ResponseValidAnalysis(send, response);
    }

    /// <summary>
    /// 初始化串口信息，9600波特率，7位数据位，1位停止位，偶校验。
    /// </summary>
    /// <param name="portName">端口号信息，例如"COM3"</param>
    public override void SerialPortInni(string portName)
    {
        base.SerialPortInni(portName);
    }

    /// <summary>
    /// 初始化串口信息，波特率，7位数据位，1位停止位，偶校验。
    /// </summary>
    /// <param name="portName">端口号信息，例如"COM3"</param>
    /// <param name="baudRate">波特率</param>
    public override void SerialPortInni(string portName, int baudRate)
    {
        base.SerialPortInni(portName, baudRate, 7, StopBits.One, Parity.Even);
    }

    /// <summary>
    /// 批量读取数据
    /// </summary>
    /// <param name="addresses">数据地址集合</param>
    /// <returns></returns>
    public Task<OperateResult<byte[]>> ReadAsync(string[] addresses)
    {
        return OmronHostLinkHelper.ReadAsync(this, addresses);
    }

    public override Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return OmronHostLinkHelper.ReadAsync(this, address, length);
    }

    public override Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return OmronHostLinkHelper.ReadBoolAsync(this, address, length);
    }

    public override Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        return OmronHostLinkHelper.WriteAsync(this, address, data);
    }

    public override Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return OmronHostLinkHelper.WriteAsync(this, address, values);
    }

    public override string ToString()
    {
        return $"OmronHostLink[{PortName}:{BaudRate}]";
    }
}
