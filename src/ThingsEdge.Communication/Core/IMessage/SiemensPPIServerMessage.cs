namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 西门子PPI的消息信息
/// </summary>
public class SiemensPPIServerMessage : NetMessageBase, INetMessage
{
    /// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
    public int ProtocolHeadBytesLength => 6;

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
    public int GetContentLengthByHeadBytes()
    {
        return HeadBytes[1];
    }
}
