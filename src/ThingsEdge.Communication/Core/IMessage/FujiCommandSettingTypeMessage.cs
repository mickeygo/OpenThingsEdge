namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// Fuji的CommandSettingType的消息类
/// </summary>
public class FujiCommandSettingTypeMessage : NetMessageBase, INetMessage
{
    public int ProtocolHeadBytesLength => 5;

    public int GetContentLengthByHeadBytes()
    {
        if (HeadBytes == null)
        {
            return 0;
        }
        return HeadBytes[4];
    }
}
