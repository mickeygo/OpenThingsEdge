namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 富士SPH协议的报文消息
/// </summary>
public class FujiSPHMessage : NetMessageBase, INetMessage
{
    /// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
    public int ProtocolHeadBytesLength => 20;

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
    public int GetContentLengthByHeadBytes()
    {
        if (HeadBytes == null)
        {
            return 0;
        }
        return BitConverter.ToUInt16(HeadBytes, 18);
    }
}
