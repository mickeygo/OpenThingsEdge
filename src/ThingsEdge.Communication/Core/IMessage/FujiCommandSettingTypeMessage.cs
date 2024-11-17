namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// Fuji的CommandSettingType的消息类
/// </summary>
public class FujiCommandSettingTypeMessage : NetMessageBase, INetMessage
{
    /// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
    public int ProtocolHeadBytesLength => 5;

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
    public int GetContentLengthByHeadBytes()
    {
        if (HeadBytes == null)
        {
            return 0;
        }
        return HeadBytes[4];
    }
}
