using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Secs.Types;

namespace ThingsEdge.Communication.Secs.Helper;

/// <summary>
/// Secs2相关的规则
/// </summary>
public static class Secs2
{
    /// <summary>
    /// 列表的类型信息
    /// </summary>
    public const int TypeList = 0;

    /// <summary>
    /// ASCII字符串的信息
    /// </summary>
    public const int TypeASCII = 64;

    /// <summary>
    /// 有符号的1个字节长度的整型
    /// </summary>
    public const int TypeSByte = 100;

    /// <summary>
    /// 无符号的1个字节长度的整型
    /// </summary>
    public const int TypeByte = 164;

    /// <summary>
    /// 有符号的2个字节长度的整型
    /// </summary>
    public const int TypeInt16 = 104;

    /// <summary>
    /// 无符号的2个字节长度的整型
    /// </summary>
    public const int TypeUInt16 = 168;

    /// <summary>
    /// 有符号的4个字节长度的整型
    /// </summary>
    public const int TypeInt32 = 112;

    /// <summary>
    /// 无符号的4个字节长度的整型
    /// </summary>
    public const int TypeUInt32 = 176;

    /// <summary>
    /// 有符号的8个字节长度的整型
    /// </summary>
    public const int TypeInt64 = 96;

    /// <summary>
    /// 无符号的8个字节长度的整型
    /// </summary>
    public const int TypeUInt64 = 160;

    /// <summary>
    /// 单浮点精度的类型
    /// </summary>
    public const int TypeSingle = 144;

    /// <summary>
    /// 双浮点精度的类型
    /// </summary>
    public const int TypeDouble = 128;

    /// <summary>
    /// Bool值信息
    /// </summary>
    public const int TypeBool = 36;

    /// <summary>
    /// 二进制的数据信息
    /// </summary>
    public const int TypeBinary = 32;

    /// <summary>
    /// JIS8类型的数据
    /// </summary>
    public const int TypeJIS8 = 68;

    /// <summary>
    /// SECS的字节顺序信息
    /// </summary>
    public static IByteTransform SecsTransform = new ReverseBytesTransform();

    internal static SecsValue ExtraToSecsItemValue(IByteTransform byteTransform, byte[] buffer, ref int index, Encoding encoding)
    {
        if (index >= buffer.Length)
        {
            return new SecsValue();
        }

        var num = buffer[index] & 3;
        var num2 = buffer[index] & 0xFC;
        var num3 = 0;
        switch (num)
        {
            case 1:
                num3 = buffer[index + 1];
                index += 2;
                break;
            case 2:
                num3 = buffer[index + 1] * 256 + buffer[index + 2];
                index += 3;
                break;
            case 3:
                num3 = buffer[index + 1] * 65536 + buffer[index + 2] * 256 + buffer[index + 3];
                index += 4;
                break;
            default:
                index++;
                break;
        }
        if (num2 == 0)
        {
            var array = new SecsValue[num3];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = ExtraToSecsItemValue(byteTransform, buffer, ref index, encoding);
            }
            return new SecsValue(SecsItemType.List, array);
        }

        var num4 = index;
        index += num3;
        return num2 switch
        {
            64 => new SecsValue(SecsItemType.ASCII, encoding.GetString(buffer, num4, num3), num3),
            100 => new SecsValue(SecsItemType.SByte, num3 == 1 ? (sbyte)buffer[num4] : (from m in buffer.SelectMiddle(num4, num3)
                                                                                        select (sbyte)m).ToArray()),
            164 => new SecsValue(SecsItemType.Byte, num3 == 1 ? buffer[num4] : buffer.SelectMiddle(num4, num3)),
            104 => new SecsValue(SecsItemType.Int16, num3 == 2 ? byteTransform.TransInt16(buffer, num4) : byteTransform.TransInt16(buffer, num4, num3 / 2)),
            168 => new SecsValue(SecsItemType.UInt16, num3 == 2 ? byteTransform.TransUInt16(buffer, num4) : byteTransform.TransUInt16(buffer, num4, num3 / 2)),
            112 => new SecsValue(SecsItemType.Int32, num3 == 4 ? byteTransform.TransInt32(buffer, num4) : byteTransform.TransInt32(buffer, num4, num3 / 4)),
            176 => new SecsValue(SecsItemType.UInt32, num3 == 4 ? byteTransform.TransUInt32(buffer, num4) : byteTransform.TransUInt32(buffer, num4, num3 / 4)),
            96 => new SecsValue(SecsItemType.Int64, num3 == 8 ? byteTransform.TransInt64(buffer, num4) : byteTransform.TransInt64(buffer, num4, num3 / 8)),
            160 => new SecsValue(SecsItemType.UInt64, num3 == 8 ? byteTransform.TransUInt64(buffer, num4) : byteTransform.TransUInt64(buffer, num4, num3 / 8)),
            144 => new SecsValue(SecsItemType.Single, num3 == 4 ? byteTransform.TransSingle(buffer, num4) : byteTransform.TransSingle(buffer, num4, num3 / 4)),
            128 => new SecsValue(SecsItemType.Double, num3 == 8 ? byteTransform.TransDouble(buffer, num4) : byteTransform.TransDouble(buffer, num4, num3 / 8)),
            36 => new SecsValue(SecsItemType.Bool, num3 == 1 ? buffer[num4] != 0 : (from m in buffer.SelectMiddle(num4, num3)
                                                                                    select m != 0).ToArray()),
            32 => new SecsValue(SecsItemType.Binary, buffer.SelectMiddle(num4, num3)),
            68 => new SecsValue(SecsItemType.JIS8, buffer.SelectMiddle(num4, num3)),
            _ => null,
        };
    }

    internal static int GetTypeCodeFrom(SecsItemType type)
    {
        return type switch
        {
            SecsItemType.ASCII => 64,
            SecsItemType.SByte => 100,
            SecsItemType.Byte => 164,
            SecsItemType.Int16 => 104,
            SecsItemType.UInt16 => 168,
            SecsItemType.Int32 => 112,
            SecsItemType.UInt32 => 176,
            SecsItemType.Int64 => 96,
            SecsItemType.UInt64 => 160,
            SecsItemType.Single => 144,
            SecsItemType.Double => 128,
            SecsItemType.Bool => 36,
            SecsItemType.Binary => 32,
            SecsItemType.JIS8 => 68,
            _ => 0,
        };
    }

    internal static void AddCodeSource(List<byte> bytes, SecsItemType type, int length)
    {
        var typeCodeFrom = GetTypeCodeFrom(type);
        if (length < 256)
        {
            bytes.Add((byte)((uint)typeCodeFrom | 1u));
            bytes.Add((byte)length);
        }
        else if (length < 65536)
        {
            var bytes2 = BitConverter.GetBytes(length);
            bytes.Add((byte)((uint)typeCodeFrom | 2u));
            bytes.Add(bytes2[1]);
            bytes.Add(bytes2[0]);
        }
        else
        {
            var bytes3 = BitConverter.GetBytes(length);
            bytes.Add((byte)((uint)typeCodeFrom | 3u));
            bytes.Add(bytes3[2]);
            bytes.Add(bytes3[1]);
            bytes.Add(bytes3[0]);
        }
    }

    internal static void AddCodeAndValueSource(List<byte> bytes, SecsValue value, Encoding encoding)
    {
        if (value != null)
        {
            if (value.ItemType == SecsItemType.List)
            {
                var length = value.Value is IEnumerable<SecsValue> source ? source.Count() : 0;
                AddCodeSource(bytes, value.ItemType, length);
                return;
            }

            var array = value.ItemType switch
            {
                SecsItemType.ASCII => encoding.GetBytes(value.Value.ToString()!),
                SecsItemType.SByte => !(value.Value.GetType() == typeof(sbyte)) ? ((sbyte[])value.Value).Select((m) => (byte)m).ToArray() : [(byte)value.Value],
                SecsItemType.Byte => !(value.Value.GetType() == typeof(byte)) ? (byte[])value.Value : [(byte)value.Value],
                SecsItemType.Int16 => value.Value.GetType() == typeof(short) ? SecsTransform.TransByte((short)value.Value) : SecsTransform.TransByte((short[])value.Value),
                SecsItemType.UInt16 => value.Value.GetType() == typeof(ushort) ? SecsTransform.TransByte((ushort)value.Value) : SecsTransform.TransByte((ushort[])value.Value),
                SecsItemType.Int32 => value.Value.GetType() == typeof(int) ? SecsTransform.TransByte((int)value.Value) : SecsTransform.TransByte((int[])value.Value),
                SecsItemType.UInt32 => value.Value.GetType() == typeof(uint) ? SecsTransform.TransByte((uint)value.Value) : SecsTransform.TransByte((uint[])value.Value),
                SecsItemType.Int64 => value.Value.GetType() == typeof(long) ? SecsTransform.TransByte((long)value.Value) : SecsTransform.TransByte((long[])value.Value),
                SecsItemType.UInt64 => value.Value.GetType() == typeof(ulong) ? SecsTransform.TransByte((ulong)value.Value) : SecsTransform.TransByte((ulong[])value.Value),
                SecsItemType.Single => value.Value.GetType() == typeof(float) ? SecsTransform.TransByte((float)value.Value) : SecsTransform.TransByte((float[])value.Value),
                SecsItemType.Double => value.Value.GetType() == typeof(double) ? SecsTransform.TransByte((double)value.Value) : SecsTransform.TransByte((double[])value.Value),
                SecsItemType.Bool => !(value.Value.GetType() == typeof(bool)) ? ((bool[])value.Value).Select((m) => (byte)(m ? byte.MaxValue : 0)).ToArray() : !(bool)value.Value ? new byte[1] : new byte[1] { 255 },
                SecsItemType.Binary => (byte[])value.Value,
                SecsItemType.JIS8 => (byte[])value.Value,
                SecsItemType.None => [],
                _ => (byte[])value.Value,
            };
            AddCodeSource(bytes, value.ItemType, array.Length);
            bytes.AddRange(array);
        }
    }

    /// <summary>
    /// 将返回的数据内容解析为实际的字符串信息，根据secsⅡ 协议定义的规则解析出实际的数据信息
    /// </summary>
    /// <param name="buffer">原始的字节数据内容</param>
    /// <param name="encoding">字符串对象的编码信息</param>
    /// <returns>字符串消息</returns>
    public static SecsValue ExtraToSecsItemValue(byte[] buffer, Encoding encoding)
    {
        if (buffer == null)
        {
            return null;
        }
        var index = 0;
        return ExtraToSecsItemValue(SecsTransform, buffer, ref index, encoding);
    }
}
