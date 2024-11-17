using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.HslCommunication;
using ThingsEdge.Communication.Profinet.Melsec.Helper;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 三菱PLC通讯类，采用Qna兼容3E帧协议实现，需要在PLC侧先的以太网模块先进行配置，必须为ASCII通讯格式<br />
/// Mitsubishi PLC communication class is implemented using Qna compatible 3E frame protocol. 
/// The Ethernet module on the PLC side needs to be configured first. It must be ascii communication.
/// </summary>
/// <remarks>
/// <inheritdoc cref="T:HslCommunication.Profinet.Melsec.MelsecMcNet" path="remarks" />
/// </remarks>
/// <example>
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecAscii.cs" region="Usage" title="简单的短连接使用" />
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecAscii.cs" region="Usage2" title="简单的长连接使用" />
/// </example>
public class MelsecMcAsciiNet : MelsecMcNet
{
    /// <inheritdoc cref="P:HslCommunication.Profinet.Melsec.Helper.IReadWriteMc.McType" />
    public override McType McType => McType.MCAscii;

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.#ctor" />
    public MelsecMcAsciiNet()
    {
        WordLength = 1;
        ByteTransform = new RegularByteTransform();
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.#ctor(System.String,System.Int32)" />
    public MelsecMcAsciiNet(string ipAddress, int port)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new MelsecQnA3EAsciiMessage();
    }

    /// <inheritdoc />
    public override byte[] PackCommandWithHeader(byte[] command)
    {
        return McAsciiHelper.PackMcCommand(this, command);
    }

    /// <inheritdoc />
    public override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        var operateResult = McAsciiHelper.CheckResponseContent(response);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        return OperateResult.CreateSuccessResult(response.RemoveBegin(22));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.IReadWriteMc.ExtractActualData(System.Byte[],System.Boolean)" />
    public override byte[] ExtractActualData(byte[] response, bool isBit)
    {
        return McAsciiHelper.ExtractActualDataHelper(response, isBit);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"MelsecMcAsciiNet[{IpAddress}:{Port}]";
    }
}
