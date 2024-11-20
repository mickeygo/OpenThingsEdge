using ThingsEdge.Communication.ModBus;

namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 富士SPB的消息内容
/// </summary>
public class FujiSPBMessage : NetMessageBase, INetMessage
{
    public int ProtocolHeadBytesLength => 5;

    public int GetContentLengthByHeadBytes()
    {
        if (HeadBytes == null)
        {
            return 0;
        }
        return Convert.ToInt32(Encoding.ASCII.GetString(HeadBytes, 3, 2), 16) * 2 + 2;
    }

    public override bool CheckReceiveDataComplete(byte[] send, MemoryStream ms)
    {
        return ModbusInfo.CheckAsciiReceiveDataComplete(ms.ToArray());
    }
}
