using ThingsEdge.Communication.HslCommunication;
using ThingsEdge.Communication.Secs.Helper;

namespace ThingsEdge.Communication.Secs.Types;

/// <summary>
/// Secs的消息类对象
/// </summary>
public class SecsMessage
{
    /// <summary>
    /// 设备的ID信息
    /// </summary>
    public ushort DeviceID { get; set; }

    /// <summary>
    /// R=false, Host → Equipment; R=true, Host ← Equipment
    /// </summary>
    public bool R { get; set; }

    /// <summary>
    /// W=false, 不必回复讯息；W=true, 必须回复讯息
    /// </summary>
    public bool W { get; set; }

    /// <summary>
    /// E=false, 尚有Block; E=true, 此为最后一个Block
    /// </summary>
    public bool E { get; set; }

    /// <summary>
    /// Stream功能码
    /// </summary>
    public byte StreamNo { get; set; }

    /// <summary>
    /// Function功能码
    /// </summary>
    public byte FunctionNo { get; set; }

    /// <summary>
    /// 获取或设置区块号信息
    /// </summary>
    public int BlockNo { get; set; }

    /// <summary>
    /// 获取或设置消息ID信息
    /// </summary>
    public uint MessageID { get; set; }

    /// <summary>
    /// 消息数据对象
    /// </summary>
    public byte[] Data { get; set; }

    /// <summary>
    /// 获取或设置用于字符串解析的编码信息
    /// </summary>
    public Encoding StringEncoding { get; set; } = Encoding.Default;

    /// <inheritdoc cref="M:HslCommunication.Secs.Types.SecsMessage.#ctor(System.Byte[],System.Int32)" />
    public SecsMessage(byte[] message)
        : this(message, 0)
    {
    }

    /// <summary>
    /// 通过原始的报文信息来实例化一个默认的对象
    /// </summary>
    /// <param name="message">原始的字节信息</param>
    /// <param name="startIndex">起始的偏移地址</param>
    public SecsMessage(byte[] message, int startIndex)
    {
        DeviceID = BitConverter.ToUInt16(
        [
            message[startIndex + 1],
            (byte)(message[startIndex] & 0x7Fu)
        ], 0);
        R = (message[startIndex] & 0x80) == 128;
        W = (message[startIndex + 2] & 0x80) == 128;
        E = (message[startIndex + 4] & 0x80) == 128;
        StreamNo = (byte)(message[startIndex + 2] & 0x7Fu);
        FunctionNo = message[startIndex + 3];
        var buffer = new byte[2]
        {
            (byte)(message[startIndex + 4] & 0x7Fu),
            message[startIndex + 5]
        };
        BlockNo = Secs2.SecsTransform.TransInt16(buffer, 0);
        MessageID = Secs2.SecsTransform.TransUInt32(message, startIndex + 6);
        Data = message.RemoveBegin(startIndex + 10);
    }

    /// <summary>
    /// 获取当前消息的所有对象信息
    /// </summary>
    /// <returns>Secs数据对象</returns>
    public SecsValue GetItemValues()
    {
        return Secs2.ExtraToSecsItemValue(Data, StringEncoding);
    }

    /// <summary>
    /// 使用指定的编码获取当前消息的所有对象信息
    /// </summary>
    /// <param name="encoding">自定义的编码信息</param>
    /// <returns>Secs数据对象</returns>
    public SecsValue GetItemValues(Encoding encoding)
    {
        return Secs2.ExtraToSecsItemValue(Data, encoding);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var itemValues = GetItemValues(StringEncoding);
        if (StreamNo == 0 && FunctionNo == 0)
        {
            return string.Format("S{0}F{1}{2} B{3}", StreamNo, FunctionNo, W ? "W" : string.Empty, BlockNo);
        }
        if (itemValues == null)
        {
            return string.Format("S{0}F{1}{2}", StreamNo, FunctionNo, W ? "W" : string.Empty);
        }
        return string.Format("S{0}F{1}{2} {3}{4}", StreamNo, FunctionNo, W ? "W" : string.Empty, Environment.NewLine, itemValues);
    }
}
