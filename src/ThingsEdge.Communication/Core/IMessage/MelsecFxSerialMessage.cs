using ThingsEdge.Communication.Profinet.Melsec.Helper;

namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 三菱的编程口的消息类。
/// </summary>
public class MelsecFxSerialMessage : NetMessageBase, INetMessage
{
    public int ProtocolHeadBytesLength => -1;

    public int GetContentLengthByHeadBytes()
    {
        return 0;
    }

    public override bool CheckReceiveDataComplete(byte[] send, MemoryStream ms)
    {
        return MelsecFxSerialHelper.CheckReceiveDataComplete(ms.ToArray());
    }
}
