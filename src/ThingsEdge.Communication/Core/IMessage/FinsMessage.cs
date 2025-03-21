namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 用于欧姆龙通信的Fins协议的消息解析规则
/// </summary>
public class FinsMessage : NetMessageBase, INetMessage
{
    public int ProtocolHeadBytesLength => 16;

    public int GetContentLengthByHeadBytes()
    {
        var num = BitConverter.ToInt32(
        [
            HeadBytes[7],
            HeadBytes[6],
            HeadBytes[5],
            HeadBytes[4]
        ], 0);
        if (num > 10000)
        {
            num = 10000;
        }
        if (num < 8)
        {
            num = 8;
        }
        return num - 8;
    }

    public override bool CheckHeadBytesLegal()
    {
        if (HeadBytes == null)
        {
            return true;
        }
        if (HeadBytes[0] == 70 && HeadBytes[1] == 73 && HeadBytes[2] == 78 && HeadBytes[3] == 83)
        {
            return true;
        }
        return false;
    }

    public override int PependedUselesByteLength(byte[] headByte)
    {
        if (headByte == null)
        {
            return 0;
        }
        for (var i = 0; i < headByte.Length - 3; i++)
        {
            if (headByte[i] == 70 && headByte[i + 1] == 73 && headByte[i + 2] == 78 && headByte[i + 3] == 83)
            {
                return i;
            }
        }
        return base.PependedUselesByteLength(headByte);
    }

    public override int CheckMessageMatch(byte[] send, byte[] receive)
    {
        if (send == null || receive == null)
        {
            return 1;
        }
        if (send.Length > 25 && receive.Length > 25)
        {
            if (send[25] == receive[25])
            {
                return 1;
            }
            return -1;
        }
        return base.CheckMessageMatch(send, receive);
    }
}
