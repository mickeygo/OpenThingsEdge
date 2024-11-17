namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 西门子S7协议的消息解析规则
/// </summary>
public class S7Message : NetMessageBase, INetMessage
{
    /// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
    public int ProtocolHeadBytesLength => 4;

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.CheckHeadBytesLegal(System.Byte[])" />
    public override bool CheckHeadBytesLegal(byte[] token)
    {
        if (HeadBytes == null)
        {
            return false;
        }
        if (HeadBytes[0] == 3 && HeadBytes[1] == 0)
        {
            return true;
        }
        return false;
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
    public int GetContentLengthByHeadBytes()
    {
        var headBytes = HeadBytes;
        if (headBytes != null && headBytes.Length >= 4)
        {
            var num = HeadBytes[2] * 256 + HeadBytes[3] - 4;
            if (num < 0)
            {
                num = 0;
            }
            return num;
        }
        return 0;
    }
}
