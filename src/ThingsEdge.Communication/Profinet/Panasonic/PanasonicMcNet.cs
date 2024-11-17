using ThingsEdge.Communication.Core.Address;
using ThingsEdge.Communication.HslCommunication;
using ThingsEdge.Communication.Profinet.Melsec;

namespace ThingsEdge.Communication.Profinet.Panasonic;

/// <summary>
/// 松下PLC的数据读写类，基于MC协议的实现，具体的地址格式请参考备注说明<br />
/// Data reading and writing of Panasonic PLC, based on the implementation of the MC protocol, please refer to the note for specific address format
/// </summary>
public class PanasonicMcNet : MelsecMcNet
{
    /// <summary>
    /// 实例化松下的的Qna兼容3E帧协议的通讯对象<br />
    /// Instantiate Panasonic's Qna compatible 3E frame protocol communication object
    /// </summary>
    public PanasonicMcNet()
    {
    }

    /// <summary>
    /// 指定ip地址及端口号来实例化一个松下的Qna兼容3E帧协议的通讯对象<br />
    /// Specify an IP address and port number to instantiate a Panasonic Qna compatible 3E frame protocol communication object
    /// </summary>
    /// <param name="ipAddress">PLC的Ip地址</param>
    /// <param name="port">PLC的端口</param>
    public PanasonicMcNet(string ipAddress, int port)
        : base(ipAddress, port)
    {
    }

    /// <inheritdoc />
    public override OperateResult<McAddressData> McAnalysisAddress(string address, ushort length, bool isBit)
    {
        return McAddressData.ParsePanasonicFrom(address, length, isBit);
    }

    /// <inheritdoc />
    public override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        var num = BitConverter.ToUInt16(response, 9);
        if (num != 0)
        {
            return new OperateResult<byte[]>(num, PanasonicHelper.GetMcErrorDescription(num));
        }
        return OperateResult.CreateSuccessResult(response.RemoveBegin(11));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"PanasonicMcNet[{IpAddress}:{Port}]";
    }
}
