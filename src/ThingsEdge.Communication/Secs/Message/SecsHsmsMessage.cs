using ThingsEdge.Communication.Core.IMessage;

namespace ThingsEdge.Communication.Secs.Message;

/// <summary>
/// Hsms协议的消息定义
/// </summary>
public class SecsHsmsMessage : NetMessageBase, INetMessage
{
    /// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
    public int ProtocolHeadBytesLength => 4;

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
    public int GetContentLengthByHeadBytes()
    {
        var num = BitConverter.ToInt32(
        [
            HeadBytes[3],
            HeadBytes[2],
            HeadBytes[1],
            HeadBytes[0]
        ], 0);
        if (num < 0)
        {
            return 0;
        }
        return num;
    }

    /// <inheritdoc />
    public override bool CheckHeadBytesLegal(byte[] token)
    {
        return true;
    }
}
