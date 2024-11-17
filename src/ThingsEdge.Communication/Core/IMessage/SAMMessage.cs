namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// SAM身份证通信协议的消息
/// </summary>
public class SAMMessage : NetMessageBase, INetMessage
{
    /// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
    public int ProtocolHeadBytesLength => 7;

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.CheckHeadBytesLegal(System.Byte[])" />
    public override bool CheckHeadBytesLegal(byte[] token)
    {
        if (HeadBytes == null)
        {
            return true;
        }
        return HeadBytes[0] == 170 && HeadBytes[1] == 170 && HeadBytes[2] == 170 && HeadBytes[3] == 150 && HeadBytes[4] == 105;
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.SAMMessage.GetContentLengthByHeadBytes" />
    public int GetContentLengthByHeadBytes()
    {
        var headBytes = HeadBytes;
        if (headBytes != null && headBytes.Length >= 7)
        {
            return HeadBytes[5] * 256 + HeadBytes[6];
        }
        return 0;
    }
}
