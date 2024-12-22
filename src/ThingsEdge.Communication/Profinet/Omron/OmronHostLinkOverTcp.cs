using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Profinet.Omron.Helper;

namespace ThingsEdge.Communication.Profinet.Omron;

/// <summary>
/// 欧姆龙的HostLink协议的实现，基于Tcp实现，地址支持示例 DM区:D100; CIO区:C100; Work区:W100; Holding区:H100; Auxiliary区: A100。
/// </summary>
/// <remarks>
/// <note type="important">
/// 如果发现串口线和usb同时打开才能通信的情况，需要按照如下的操作：
/// 串口线不是标准的串口线，电脑的串口线的235引脚分别接PLC的329引脚，45线短接，就可以通讯。
/// </note>
/// </remarks>
public class OmronHostLinkOverTcp : DeviceTcpNet, IHostLink, IReadWriteDevice, IReadWriteNet
{
    /// <summary>
    /// Specifies whether or not there are network relays. Set “80” (ASCII: 38,30) 
    /// when sending an FINS command to a CPU Unit on a network.Set “00” (ASCII: 30,30) 
    /// when sending to a CPU Unit connected directly to the host computer.
    /// </summary>
    public byte ICF { get; set; }

    public byte DA2 { get; set; }

    public byte SA2 { get; set; }

    public byte SID { get; set; }

    /// <summary>
    /// The response wait time sets the time from when the CPU Unit receives a command block until it starts 
    /// to return a response.It can be set from 0 to F in hexadecimal, in units of 10 ms.
    /// If F(15) is set, the response will begin to be returned 150 ms (15 × 10 ms) after the command block was received.
    /// </summary>
    public byte ResponseWaitTime { get; set; } = 48;

    public byte UnitNumber { get; set; }

    /// <summary>
    /// 进行字读取的时候对于超长的情况按照本属性进行切割，默认260。
    /// </summary>
    public int ReadSplits { get; set; } = 260;

    public OmronPlcType PlcType { get; set; } = OmronPlcType.CSCJ;

    public OmronHostLinkOverTcp(string ipAddress, int port, DeviceTcpNetOptions? options = null) : base(ipAddress, port, options)
    {
        ByteTransform = new RegularByteTransform(DataFormat.CDAB);
        WordLength = 1;
    }

    protected override INetMessage GetNewNetMessage()
    {
        return new SpecifiedCharacterMessage(13);
    }

    protected override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        return OmronHostLinkHelper.ResponseValidAnalysis(send, response);
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

    public override Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        return OmronHostLinkHelper.WriteAsync(this, address, data);
    }

    public override Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return OmronHostLinkHelper.ReadBoolAsync(this, address, length);
    }

    public override Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return OmronHostLinkHelper.WriteAsync(this, address, values);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"OmronHostLinkOverTcp[{Host}:{Port}]";
    }
}
