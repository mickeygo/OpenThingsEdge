namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 用于和 AllenBradley PLC 交互的消息协议类
/// </summary>
public class AllenBradleyMessage : NetMessageBase, INetMessage
{
    public int ProtocolHeadBytesLength => 24;

    public int GetContentLengthByHeadBytes()
    {
        return BitConverter.ToUInt16(HeadBytes, 2);
    }
}
