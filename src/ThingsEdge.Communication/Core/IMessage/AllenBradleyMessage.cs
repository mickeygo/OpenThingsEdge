namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 用于和 AllenBradley PLC 交互的消息协议类
/// </summary>
public class AllenBradleyMessage : NetMessageBase, INetMessage
{
    /// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
    public int ProtocolHeadBytesLength => 24;

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
    public int GetContentLengthByHeadBytes()
    {
        return BitConverter.ToUInt16(HeadBytes, 2);
    }
}
