namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 三菱的A兼容1E帧协议解析规则
/// </summary>
public class MelsecA1EBinaryMessage : NetMessageBase, INetMessage
{
    /// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
    public int ProtocolHeadBytesLength => 2;

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
    public int GetContentLengthByHeadBytes()
    {
        if (HeadBytes[1] == 91)
        {
            return 2;
        }
        if (HeadBytes[1] == 0)
        {
            switch (HeadBytes[0])
            {
                case 128:
                    return SendBytes[10] != 0 ? (SendBytes[10] + 1) / 2 : 128;
                case 129:
                    return SendBytes[10] * 2;
                case 130:
                case 131:
                    return 0;
                default:
                    return 0;
            }
        }
        return 0;
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.CheckHeadBytesLegal(System.Byte[])" />
    public override bool CheckHeadBytesLegal(byte[] token)
    {
        if (HeadBytes != null)
        {
            return HeadBytes[0] - SendBytes[0] == 128;
        }
        return false;
    }
}
