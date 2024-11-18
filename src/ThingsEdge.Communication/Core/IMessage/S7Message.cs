namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 西门子S7协议的消息解析规则。
/// </summary>
public class S7Message : NetMessageBase, INetMessage
{
    public int ProtocolHeadBytesLength => 4;

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

    public int GetContentLengthByHeadBytes()
    {
        var headBytes = HeadBytes;
        if (headBytes != null && headBytes.Length >= 4)
        {
            var num = headBytes[2] * 256 + headBytes[3] - 4;
            if (num < 0)
            {
                num = 0;
            }
            return num;
        }
        return 0;
    }
}
