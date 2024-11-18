using ThingsEdge.Communication.Common;

namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// Modbus-Tcp协议支持的消息解析类。
/// </summary>
public class ModbusTcpMessage : NetMessageBase, INetMessage
{
    public int ProtocolHeadBytesLength => 8;

    /// <summary>
    /// 获取或设置是否进行检查返回的消息ID和发送的消息ID是否一致，默认为true，也就是检查。
    /// </summary>
    public bool IsCheckMessageId { get; set; } = true;

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

    public override int GetHeadBytesIdentity()
    {
        return HeadBytes[0] * 256 + HeadBytes[1];
    }
}
