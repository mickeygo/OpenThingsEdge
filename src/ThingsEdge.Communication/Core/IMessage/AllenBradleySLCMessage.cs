namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 用于和 AllenBradley PLC 交互的消息协议类
/// </summary>
public class AllenBradleySLCMessage : NetMessageBase, INetMessage
{
    /// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
    public int ProtocolHeadBytesLength => 28;

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
    public int GetContentLengthByHeadBytes()
    {
        if (HeadBytes == null)
        {
            return 0;
        }
        return HeadBytes[2] * 256 + HeadBytes[3];
    }
}
