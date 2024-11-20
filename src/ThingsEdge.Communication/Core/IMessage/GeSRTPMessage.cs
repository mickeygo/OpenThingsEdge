namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 通用电气公司的SRIP协议的消息
/// </summary>
public class GeSRTPMessage : NetMessageBase, INetMessage
{
    public int ProtocolHeadBytesLength => 56;

    public int GetContentLengthByHeadBytes()
    {
        return HeadBytes[4] + HeadBytes[5] * 256;
    }
}
