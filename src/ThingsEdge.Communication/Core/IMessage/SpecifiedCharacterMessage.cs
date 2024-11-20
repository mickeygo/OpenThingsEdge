namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 专门用于接收指定字符结尾的网络消息
/// </summary>
public class SpecifiedCharacterMessage : NetMessageBase, INetMessage
{
    private int _protocolHeadBytesLength = -1;

    /// <summary>
    /// 获取或设置在结束字符之后剩余的固定字节长度，有些则还包含两个字节的校验码，这时该值就需要设置为2。
    /// </summary>
    public byte EndLength
    {
        get => BitConverter.GetBytes(_protocolHeadBytesLength)[2];
        set
        {
            var bytes = BitConverter.GetBytes(_protocolHeadBytesLength);
            bytes[2] = value;
            _protocolHeadBytesLength = BitConverter.ToInt32(bytes, 0);
        }
    }

    public int ProtocolHeadBytesLength => _protocolHeadBytesLength;

    /// <summary>
    /// 使用固定的一个字符结尾作为当前的报文接收条件，来实例化一个对象。
    /// </summary>
    /// <param name="endCode">结尾的字符</param>
    public SpecifiedCharacterMessage(byte endCode)
    {
        var array = new byte[4];
        array[3] = (byte)(array[3] | 0x80u);
        array[3] = (byte)(array[3] | 1u);
        array[1] = endCode;
        _protocolHeadBytesLength = BitConverter.ToInt32(array, 0);
    }

    /// <summary>
    /// 使用固定的两个个字符结尾作为当前的报文接收条件，来实例化一个对象。
    /// </summary>
    /// <param name="endCode1">第一个结尾的字符</param>
    /// <param name="endCode2">第二个结尾的字符</param>
    public SpecifiedCharacterMessage(byte endCode1, byte endCode2)
    {
        var array = new byte[4];
        array[3] = (byte)(array[3] | 0x80u);
        array[3] = (byte)(array[3] | 2u);
        array[1] = endCode1;
        array[0] = endCode2;
        _protocolHeadBytesLength = BitConverter.ToInt32(array, 0);
    }

    public int GetContentLengthByHeadBytes()
    {
        return 0;
    }

    public override bool CheckHeadBytesLegal(byte[] token)
    {
        return true;
    }
}
