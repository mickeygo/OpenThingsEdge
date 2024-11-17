using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// Modbus-Tcp协议支持的消息解析类
/// </summary>
public class ModbusTcpMessage : NetMessageBase, INetMessage
{
    /// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
    public int ProtocolHeadBytesLength => 8;

    /// <summary>
    /// 获取或设置是否进行检查返回的消息ID和发送的消息ID是否一致，默认为true，也就是检查<br />
    /// Get or set whether to check whether the returned message ID is consistent with the sent message ID, the default is true, that is, check
    /// </summary>
    public bool IsCheckMessageId { get; set; } = true;


    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
    public int GetContentLengthByHeadBytes()
    {
        if (HeadBytes?.Length >= ProtocolHeadBytesLength)
        {
            var num = HeadBytes[4] * 256 + HeadBytes[5];
            if (num == 0)
            {
                HeadBytes = HeadBytes.RemoveBegin(1);
                return HeadBytes[4] * 256 + HeadBytes[5] - 1;
            }
            return Math.Min(num - 2, 300);
        }
        return 0;
    }

    /// <inheritdoc />
    public override int CheckMessageMatch(byte[] send, byte[] receive)
    {
        if (!IsCheckMessageId)
        {
            return 1;
        }
        if (send == null)
        {
            return 1;
        }
        if (receive == null)
        {
            return 1;
        }
        if (send.Length < 8 || receive.Length < 8)
        {
            return 1;
        }
        if (send[0] == receive[0] && send[1] == receive[1])
        {
            return 1;
        }
        return -1;
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetHeadBytesIdentity" />
    public override int GetHeadBytesIdentity()
    {
        return HeadBytes[0] * 256 + HeadBytes[1];
    }
}
