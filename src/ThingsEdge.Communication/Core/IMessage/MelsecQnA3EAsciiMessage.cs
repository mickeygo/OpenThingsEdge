namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 基于MC协议的Qna兼容3E帧协议的ASCII通讯消息机制
/// </summary>
public class MelsecQnA3EAsciiMessage : NetMessageBase, INetMessage
{
    /// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
    public int ProtocolHeadBytesLength => 18;

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
    public int GetContentLengthByHeadBytes()
    {
        var bytes = new byte[4]
        {
            HeadBytes[14],
            HeadBytes[15],
            HeadBytes[16],
            HeadBytes[17]
        };
        return Convert.ToInt32(Encoding.ASCII.GetString(bytes), 16);
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.CheckHeadBytesLegal(System.Byte[])" />
    public override bool CheckHeadBytesLegal(byte[] token)
    {
        if (HeadBytes == null)
        {
            return false;
        }
        if (HeadBytes[0] == 68 && HeadBytes[1] == 48 && HeadBytes[2] == 48 && HeadBytes[3] == 48)
        {
            return true;
        }
        return false;
    }
}
