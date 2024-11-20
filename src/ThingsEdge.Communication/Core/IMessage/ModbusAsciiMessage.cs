using ThingsEdge.Communication.ModBus;

namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// ModbusAscii消息类对象
/// </summary>
public class ModbusAsciiMessage : NetMessageBase, INetMessage
{
    public int ProtocolHeadBytesLength => -1;

    public int GetContentLengthByHeadBytes()
    {
        return 0;
    }

    public override bool CheckReceiveDataComplete(byte[] send, MemoryStream ms)
    {
        return ModbusInfo.CheckAsciiReceiveDataComplete(ms.ToArray());
    }
}
