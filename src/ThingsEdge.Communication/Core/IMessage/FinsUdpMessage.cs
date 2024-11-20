namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// FinsUdp的消息
/// </summary>
public class FinsUdpMessage : NetMessageBase, INetMessage
{
    public int ProtocolHeadBytesLength => -1;

    public int GetContentLengthByHeadBytes()
    {
        return 0;
    }

    public override int CheckMessageMatch(byte[] send, byte[] receive)
    {
        if (send == null || receive == null)
        {
            return 1;
        }

        if (send.Length >= 10 && receive.Length >= 10)
        {
            if (send[9] == receive[9])
            {
                return 1;
            }
            return -1;
        }
        return base.CheckMessageMatch(send, receive);
    }
}
