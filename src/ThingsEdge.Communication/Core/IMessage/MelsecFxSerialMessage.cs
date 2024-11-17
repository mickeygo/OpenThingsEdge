using ThingsEdge.Communication.Profinet.Melsec.Helper;

namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 三菱的编程口的消息类
/// </summary>
public class MelsecFxSerialMessage : NetMessageBase, INetMessage
{
    /// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
    public int ProtocolHeadBytesLength => -1;

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
    public int GetContentLengthByHeadBytes()
    {
        return 0;
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.CheckReceiveDataComplete(System.Byte[],System.IO.MemoryStream)" />
    public override bool CheckReceiveDataComplete(byte[] send, MemoryStream ms)
    {
        return MelsecFxSerialHelper.CheckReceiveDataComplete(ms.ToArray());
    }
}
