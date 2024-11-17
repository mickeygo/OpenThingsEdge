namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 发那科机器人的网络消息类
/// </summary>
public class FanucRobotMessage : NetMessageBase, INetMessage
{
    /// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
    public int ProtocolHeadBytesLength => 56;

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
    public int GetContentLengthByHeadBytes()
    {
        return BitConverter.ToUInt16(HeadBytes, 4);
    }
}
