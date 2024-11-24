using ThingsEdge.Communication.Core.Address;
using ThingsEdge.Communication.Profinet.Melsec;

namespace ThingsEdge.Communication.Profinet.Panasonic;

/// <summary>
/// 松下PLC的数据读写类，基于MC协议的实现。
/// </summary>
public class PanasonicMcNet : MelsecMcNet
{
    /// <summary>
    /// 指定ip地址及端口号来实例化一个松下的Qna兼容3E帧协议的通讯对象。
    /// </summary>
    /// <param name="ipAddress">PLC的Ip地址</param>
    /// <param name="port">PLC的端口</param>
    public PanasonicMcNet(string ipAddress, int port)
        : base(ipAddress, port)
    {
    }

    public override OperateResult<McAddressData> McAnalysisAddress(string address, ushort length, bool isBit)
    {
        return McAddressData.ParsePanasonicFrom(address, length, isBit);
    }

    protected override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        var num = BitConverter.ToUInt16(response, 9);
        if (num != 0)
        {
            return new OperateResult<byte[]>(num, PanasonicHelper.GetMcErrorDescription(num));
        }
        return OperateResult.CreateSuccessResult(response.RemoveBegin(11));
    }

    public override string ToString()
    {
        return $"PanasonicMcNet[{IpAddress}:{Port}]";
    }
}
