using ThingsEdge.Communication.ModBus;

namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// ModbusAscii消息类对象
/// </summary>
public class ModbusAsciiMessage : NetMessageBase, INetMessage
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
        return ModbusInfo.CheckAsciiReceiveDataComplete(ms.ToArray());
    }
}
