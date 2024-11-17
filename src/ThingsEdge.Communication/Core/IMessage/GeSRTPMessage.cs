namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 通用电气公司的SRIP协议的消息
/// </summary>
public class GeSRTPMessage : NetMessageBase, INetMessage
{
    /// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
    public int ProtocolHeadBytesLength => 56;

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
    public int GetContentLengthByHeadBytes()
    {
        return HeadBytes[4] + HeadBytes[5] * 256;
    }
}
