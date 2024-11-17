using ThingsEdge.Communication.ModBus;

namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 富士SPB的消息内容
/// </summary>
public class FujiSPBMessage : NetMessageBase, INetMessage
{
    /// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
    public int ProtocolHeadBytesLength => 5;

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
    public int GetContentLengthByHeadBytes()
    {
        if (HeadBytes == null)
        {
            return 0;
        }
        return Convert.ToInt32(Encoding.ASCII.GetString(HeadBytes, 3, 2), 16) * 2 + 2;
    }

    /// <inheritdoc />
    public override bool CheckReceiveDataComplete(byte[] send, MemoryStream ms)
    {
        return ModbusInfo.CheckAsciiReceiveDataComplete(ms.ToArray());
    }
}
