namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 西门子Fetch/Write消息解析协议
/// </summary>
public class FetchWriteMessage : NetMessageBase, INetMessage
{
    /// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
    public int ProtocolHeadBytesLength => 16;

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
    public int GetContentLengthByHeadBytes()
    {
        if (HeadBytes[5] == 5 || HeadBytes[5] == 4)
        {
            return 0;
        }
        if (HeadBytes[5] == 6)
        {
            if (SendBytes == null)
            {
                return 0;
            }
            if (HeadBytes[8] != 0)
            {
                return 0;
            }
            if (SendBytes[8] == 1 || SendBytes[8] == 6 || SendBytes[8] == 7)
            {
                return (SendBytes[12] * 256 + SendBytes[13]) * 2;
            }
            return SendBytes[12] * 256 + SendBytes[13];
        }
        if (HeadBytes[5] == 3)
        {
            if (HeadBytes[8] == 1 || HeadBytes[8] == 6 || HeadBytes[8] == 7)
            {
                return (HeadBytes[12] * 256 + HeadBytes[13]) * 2;
            }
            return HeadBytes[12] * 256 + HeadBytes[13];
        }
        return 0;
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.CheckHeadBytesLegal(System.Byte[])" />
    public override bool CheckHeadBytesLegal(byte[] token)
    {
        if (HeadBytes == null)
        {
            return false;
        }
        if (HeadBytes[0] == 83 && HeadBytes[1] == 53)
        {
            return true;
        }
        return false;
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetHeadBytesIdentity" />
    public override int GetHeadBytesIdentity()
    {
        return HeadBytes[3];
    }
}
