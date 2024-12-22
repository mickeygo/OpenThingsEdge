using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Profinet.Melsec.Helper;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 三菱PLC通讯类，采用Qna兼容3E帧协议实现，需要在PLC侧先的以太网模块先进行配置，必须为ASCII通讯格式。
/// </summary>
public class MelsecMcAsciiNet : MelsecMcNet
{
    public override McType McType => McType.MCAscii;

    public MelsecMcAsciiNet(string ipAddress, int port, DeviceTcpNetOptions? options = null) : base(ipAddress, port, options)
    {
        WordLength = 1;
        ByteTransform = new RegularByteTransform();
    }

    protected override INetMessage GetNewNetMessage()
    {
        return new MelsecQnA3EAsciiMessage();
    }

    protected override byte[] PackCommandWithHeader(byte[] command)
    {
        return McAsciiHelper.PackMcCommand(this, command);
    }

    protected override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        var operateResult = McAsciiHelper.CheckResponseContent(response);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        return OperateResult.CreateSuccessResult(response.RemoveBegin(22));
    }

    public override byte[] ExtractActualData(byte[] response, bool isBit)
    {
        return McAsciiHelper.ExtractActualDataHelper(response, isBit);
    }

    public override string ToString()
    {
        return $"MelsecMcAsciiNet[{Host}:{Port}]";
    }
}
