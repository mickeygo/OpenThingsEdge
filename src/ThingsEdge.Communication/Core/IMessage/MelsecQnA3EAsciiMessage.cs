namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 基于MC协议的Qna兼容3E帧协议的ASCII通讯消息机制
/// </summary>
public class MelsecQnA3EAsciiMessage : NetMessageBase, INetMessage
{
    public int ProtocolHeadBytesLength => 18;

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

    public override bool CheckHeadBytesLegal()
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
