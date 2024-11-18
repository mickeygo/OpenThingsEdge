using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Profinet.Omron.Helper;

namespace ThingsEdge.Communication.Profinet.Omron;

/// <summary>
/// 欧姆龙的HostLink协议的实现，基于Tcp实现，地址支持示例 DM区:D100; CIO区:C100; Work区:W100; Holding区:H100; Auxiliary区: A100。
/// </summary>
/// <remarks>
/// 感谢 深圳～拾忆 的测试，地址可以携带站号信息，例如 s=2;D100 
/// <br />
/// <note type="important">
/// 如果发现串口线和usb同时打开才能通信的情况，需要按照如下的操作：<br />
/// 串口线不是标准的串口线，电脑的串口线的235引脚分别接PLC的329引脚，45线短接，就可以通讯，感谢 深圳-小君(QQ932507362)提供的解决方案。
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


    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.DA2" />
    public byte DA2 { get; set; }


    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.SA2" />
    public byte SA2 { get; set; }

    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.SID" />
    public byte SID { get; set; }

    /// <summary>
    /// The response wait time sets the time from when the CPU Unit receives a command block until it starts 
    /// to return a response.It can be set from 0 to F in hexadecimal, in units of 10 ms.
    /// If F(15) is set, the response will begin to be returned 150 ms (15 × 10 ms) after the command block was received.
    /// </summary>
    public byte ResponseWaitTime { get; set; } = 48;


    /// <summary>
    /// PLC设备的站号信息<br />
    /// PLC device station number information
    /// </summary>
    public byte UnitNumber { get; set; }

    /// <summary>
    /// 进行字读取的时候对于超长的情况按照本属性进行切割，默认260。<br />
    /// When reading words, it is cut according to this attribute for the case of overlength. The default is 260.
    /// </summary>
    public int ReadSplits { get; set; } = 260;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.Helper.IOmronFins.PlcType" />
    public OmronPlcType PlcType { get; set; } = OmronPlcType.CSCJ;


    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.#ctor" />
    public OmronHostLinkOverTcp()
    {
        ByteTransform = new RegularByteTransform(DataFormat.CDAB);
        WordLength = 1;
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronCipNet.#ctor(System.String,System.Int32)" />
    public OmronHostLinkOverTcp(string ipAddress, int port)
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

    /// <inheritdoc />
    public override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        return OmronHostLinkHelper.ResponseValidAnalysis(send, response);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.Helper.OmronHostLinkHelper.Read(HslCommunication.Profinet.Omron.Helper.IHostLink,System.String[])" />
    public OperateResult<byte[]> Read(string[] address)
    {
        return OmronHostLinkHelper.Read(this, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.Read(System.String,System.UInt16)" />
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return await OmronHostLinkHelper.ReadAsync(this, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.Write(System.String,System.Byte[])" />
    public override async Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        return await OmronHostLinkHelper.WriteAsync(this, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.Helper.OmronHostLinkHelper.Read(HslCommunication.Profinet.Omron.Helper.IHostLink,System.String[])" />
    public async Task<OperateResult<byte[]>> ReadAsync(string[] address)
    {
        return await OmronHostLinkHelper.ReadAsync(this, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.ReadBool(System.String,System.UInt16)" />
    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return await OmronHostLinkHelper.ReadBoolAsync(this, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.Write(System.String,System.Boolean[])" />
    public override async Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return await OmronHostLinkHelper.WriteAsync(this, address, values);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"OmronHostLinkOverTcp[{IpAddress}:{Port}]";
    }
}
