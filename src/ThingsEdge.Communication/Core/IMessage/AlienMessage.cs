namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 异形消息对象，用于异形客户端的注册包接收以及验证使用
/// </summary>
public class AlienMessage : NetMessageBase, INetMessage
{
    /// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
    public int ProtocolHeadBytesLength => 5;

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.CheckHeadBytesLegal(System.Byte[])" />
    public override bool CheckHeadBytesLegal(byte[] token)
    {
        if (HeadBytes == null)
        {
            return true;
        }
        if (HeadBytes[0] == 72)
        {
            return true;
        }
        return false;
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
    public int GetContentLengthByHeadBytes()
    {
        return HeadBytes[3] * 256 + HeadBytes[4];
    }
}
