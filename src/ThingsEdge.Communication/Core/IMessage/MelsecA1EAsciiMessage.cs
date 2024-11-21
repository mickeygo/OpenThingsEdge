namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 三菱的A兼容1E帧ASCII协议解析规则
/// </summary>
public class MelsecA1EAsciiMessage : NetMessageBase, INetMessage
{
    public int ProtocolHeadBytesLength => 4;

    public int GetContentLengthByHeadBytes()
    {
        if (HeadBytes[2] == 53 && HeadBytes[3] == 66)
        {
            return 4;
        }

        if (HeadBytes[2] == 48 && HeadBytes[3] == 48)
        {
            var num = Convert.ToInt32(Encoding.ASCII.GetString(SendBytes, 20, 2), 16);
            if (num == 0)
            {
                num = 256;
            }
            return HeadBytes[1] switch
            {
                48 => num % 2 == 1 ? num + 1 : num,
                49 => num * 4,
                50 or 51 => 0,
                _ => 0,
            };
        }
        return 0;
    }

    public override bool CheckHeadBytesLegal()
    {
        if (HeadBytes != null)
        {
            return HeadBytes[0] - SendBytes[0] == 8;
        }
        return false;
    }
}
