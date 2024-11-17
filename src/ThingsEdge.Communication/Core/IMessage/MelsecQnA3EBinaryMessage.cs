namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 三菱的Qna兼容3E帧协议解析规则
/// </summary>
public class MelsecQnA3EBinaryMessage : NetMessageBase, INetMessage
{
    /// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
    public int ProtocolHeadBytesLength => 9;

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
    public int GetContentLengthByHeadBytes()
    {
        return BitConverter.ToUInt16(HeadBytes, 7);
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.CheckHeadBytesLegal(System.Byte[])" />
    public override bool CheckHeadBytesLegal(byte[] token)
    {
        if (HeadBytes == null)
        {
            return false;
        }
        if (HeadBytes[0] == 208 && HeadBytes[1] == 0)
        {
            return true;
        }
        return false;
    }
}
