namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 西门子PPI的消息信息
/// </summary>
public class SiemensPPIServerMessage : NetMessageBase, INetMessage
{
    public int ProtocolHeadBytesLength => 6;

    public int GetContentLengthByHeadBytes()
    {
        return HeadBytes[1];
    }
}
