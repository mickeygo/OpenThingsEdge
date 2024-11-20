namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 富士SPH协议的报文消息
/// </summary>
public class FujiSPHMessage : NetMessageBase, INetMessage
{
    public int ProtocolHeadBytesLength => 20;

    public int GetContentLengthByHeadBytes()
    {
        if (HeadBytes == null)
        {
            return 0;
        }
        return BitConverter.ToUInt16(HeadBytes, 18);
    }
}
