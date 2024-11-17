namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// Kuka机器人的 KRC4 控制器中的服务器KUKAVARPROXY
/// </summary>
public class KukaVarProxyMessage : NetMessageBase, INetMessage
{
    /// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
    public int ProtocolHeadBytesLength => 4;

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
    public int GetContentLengthByHeadBytes()
    {
        var headBytes = HeadBytes;
        if (headBytes != null && headBytes.Length >= 4)
        {
            return HeadBytes[2] * 256 + HeadBytes[3];
        }
        return 0;
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetHeadBytesIdentity" />
    public override int GetHeadBytesIdentity()
    {
        var headBytes = HeadBytes;
        if (headBytes != null && headBytes.Length >= 4)
        {
            return HeadBytes[0] * 256 + HeadBytes[1];
        }
        return 0;
    }
}
