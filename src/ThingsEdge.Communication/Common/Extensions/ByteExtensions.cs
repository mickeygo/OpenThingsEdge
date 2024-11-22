using System.Runtime.InteropServices;

namespace ThingsEdge.Communication.Common.Extensions;

/// <summary>
/// 数据扩展的辅助类方法
/// </summary>
internal static class ByteExtensions
{
    /// <summary>
    /// 将原始的byte数组转换成ascii格式的byte数组。
    /// </summary>
    /// <param name="inBytes">等待转换的byte数组</param>
    /// <returns>转换后的数组</returns>
    public static byte[] ToAsciiBytes(this byte[] inBytes)
    {
        return Encoding.ASCII.GetBytes(ToHexString(inBytes));
    }

    /// <summary>
    /// 将字节数组显示为ASCII格式的字符串，当遇到0x20以下及0x7E以上的不可见字符时，使用十六进制的数据显示。
    /// </summary>
    /// <param name="inBytes">字节数组信息</param>
    /// <returns>ASCII格式的字符串信息</returns>
    public static string ToAsciiString(this byte[] inBytes)
    {
        var stringBuilder = new StringBuilder();
        for (var i = 0; i < inBytes.Length; i++)
        {
            if (inBytes[i] < 32 || inBytes[i] > 126)
            {
                stringBuilder.Append($"\\{inBytes[i]:X2}");
            }
            else
            {
                stringBuilder.Append((char)inBytes[i]);
            }
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 字节数据转化成16进制表示的字符串。
    /// </summary>
    /// <param name="inBytes">字节数组</param>
    /// <returns>返回的字符串</returns>
    public static string ToHexString(this byte[]? inBytes)
    {
        if (inBytes == null)
        {
            return string.Empty;
        }

        return ToHexString(inBytes, '\0');
    }

    /// <summary>
    /// 字节数据转化成16进制表示的字符串。
    /// </summary>
    /// <param name="inBytes">字节数组</param>
    /// <param name="segment">分割符</param>
    /// <returns>返回的字符串</returns>
    public static string ToHexString(this byte[]? inBytes, char segment)
    {
        if (inBytes == null)
        {
            return string.Empty;
        }

        return ToHexString(inBytes, segment, 0);
    }

    /// <summary>
    /// 字节数据转化成16进制表示的字符串。
    /// </summary>
    /// <param name="inBytes">字节数组</param>
    /// <param name="segment">分割符，如果设置为0，则没有分隔符信息</param>
    /// <param name="newLineCount">每隔指定数量的时候进行换行，如果小于等于0，则不进行分行显示</param>
    /// <returns>返回的字符串</returns>
    public static string ToHexString(this byte[] inBytes, char segment, int newLineCount)
    {
        var stringBuilder = new StringBuilder();
        var num = 0L;
        foreach (var b in inBytes)
        {
            if (segment == '\0')
            {
                stringBuilder.AppendFormat("{0:X2}", b);
            }
            else
            {
                stringBuilder.AppendFormat("{0:X2}{1}", b, segment);
            }
            num++;
            if (newLineCount > 0 && num >= newLineCount)
            {
                stringBuilder.Append(Environment.NewLine);
                num = 0L;
            }
        }
        if (segment != 0 && stringBuilder.Length > 1 && stringBuilder[stringBuilder.Length - 1] == segment)
        {
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 将bool数组转换到byte数组。
    /// </summary>
    /// <param name="array">bool数组</param>
    /// <returns>转换后的字节数组</returns>
    public static byte[] ToByteArray(this bool[] array)
    {
        var num = array.Length % 8 == 0 ? array.Length / 8 : array.Length / 8 + 1;
        var array2 = new byte[num];
        for (var i = 0; i < array.Length; i++)
        {
            if (array[i])
            {
                array2[i / 8] += GetDataByBitIndex(i % 8);
            }
        }
        return array2;
    }

    /// <summary>
    /// 从Byte数组中提取所有的位数组。
    /// </summary>
    /// <param name="inBytes">原先的字节数组</param>
    /// <returns>转换后的bool数组</returns>
    public static bool[] ToBoolArray(this byte[] inBytes)
    {
        return ToBoolArray(inBytes, inBytes.Length * 8);
    }

    /// <summary>
    /// 从Byte数组中提取位数组，length代表位数，例如数组 03 A1 长度10转为 [1100 0000 10]。
    /// </summary>
    /// <param name="inBytes">原先的字节数组</param>
    /// <param name="length">想要转换的长度，如果超出自动会缩小到数组最大长度</param>
    /// <returns>转换后的bool数组</returns>
    public static bool[] ToBoolArray(this byte[] inBytes, int length)
    {
        if (length > inBytes.Length * 8)
        {
            length = inBytes.Length * 8;
        }

        var array = new bool[length];
        for (var i = 0; i < length; i++)
        {
            array[i] = BoolOnByteIndex(inBytes[i / 8], i % 8);
        }
        return array;
    }

    /// <summary>
    /// 将原始的字节数组，转换成实际的结构体对象，需要事先定义好结构体内容，否则会转换失败。
    /// </summary>
    /// <typeparam name="T">自定义的结构体</typeparam>
    /// <param name="content">原始的字节内容</param>
    /// <param name="value">解析的数据</param>
    /// <returns>是否成功的结果对象</returns>
    public static bool ToStruct<T>(this byte[] content, [NotNullWhen(true)] out T? value) where T : struct
    {
        var num = Marshal.SizeOf(typeof(T));
        var intPtr = Marshal.AllocHGlobal(num);
        try
        {
            Marshal.Copy(content, 0, intPtr, num);
            value = Marshal.PtrToStructure<T>(intPtr);
            return true;
        }
        catch
        {
            value = null;
            return false;
        }
        finally
        {
            Marshal.FreeHGlobal(intPtr);
        }
    }

    /// <summary>
    /// 将一个数组的前后移除指定位数，返回新的一个数组。
    /// </summary>
    /// <param name="value">数组</param>
    /// <param name="leftLength">前面的位数</param>
    /// <param name="rightLength">后面的位数</param>
    /// <returns>新的数组</returns>
    public static T[] RemoveBoth<T>(this T[] value, int leftLength, int rightLength)
    {
        if (value.Length <= leftLength + rightLength)
        {
            return [];
        }

        var array = new T[value.Length - leftLength - rightLength];
        Array.Copy(value, leftLength, array, 0, array.Length);
        return array;
    }

    /// <summary>
    /// 将一个数组的前面指定位数移除，返回新的一个数组。
    /// </summary>
    /// <param name="value">数组</param>
    /// <param name="length">等待移除的长度</param>
    /// <returns>新的数组</returns>
    public static T[] RemoveBegin<T>(this T[] value, int length)
    {
        return ArrayRemoveBoth(value, length, 0);
    }

    /// <summary>
    /// 将一个数组的后面指定位数移除，返回新的一个数组。
    /// </summary>
    /// <param name="value">数组</param>
    /// <param name="length">等待移除的长度</param>
    /// <returns>新的数组</returns>
    public static T[] RemoveEnd<T>(this T[] value, int length)
    {
        return ArrayRemoveBoth(value, 0, length);
    }

    /// <summary>
    /// 将一个数组的前后移除指定位数，返回新的一个数组。
    /// </summary>
    /// <param name="value">数组</param>
    /// <param name="leftLength">前面的位数</param>
    /// <param name="rightLength">后面的位数</param>
    /// <returns>新的数组</returns>
    private static T[] ArrayRemoveBoth<T>(T[] value, int leftLength, int rightLength)
    {
        if (value.Length <= leftLength + rightLength)
        {
            return [];
        }

        var array = new T[value.Length - leftLength - rightLength];
        Array.Copy(value, leftLength, array, 0, array.Length);
        return array;
    }

    /// <summary>
    /// 获取Byte的第 offset 偏移的bool值，比如3，就是第4位。
    /// </summary>
    /// <param name="value">字节信息</param>
    /// <param name="offset">索引位置</param>
    /// <returns>bool值</returns>
    public static bool GetBoolByIndex(this byte value, int offset)
    {
        var dataByBitIndex = GetDataByBitIndex(offset);
        return (value & dataByBitIndex) == dataByBitIndex;
    }

    /// <summary>
    /// 获取Byte数组的第 boolIndex 偏移的bool值，这个偏移值可以为 10，就是第 1 个字节的 第3位。
    /// </summary>
    /// <param name="bytes">字节数组信息</param>
    /// <param name="boolIndex">指定字节的位偏移</param>
    /// <returns>bool值</returns>
    public static bool GetBoolByIndex(this byte[] bytes, int boolIndex)
    {
        return BoolOnByteIndex(bytes[boolIndex / 8], boolIndex % 8);
    }

    /// <summary>
    /// 获取到数组里面的中间指定长度的数组。
    /// </summary>
    /// <param name="value">数组</param>
    /// <param name="index">起始索引</param>
    /// <param name="length">数据的长度</param>
    /// <returns>新的数组值</returns>
    public static T[] SelectMiddle<T>(this T[] value, int index, int length)
    {
        if (length == 0)
        {
            return [];
        }

        var array = new T[Math.Min(value.Length, length)];
        Array.Copy(value, index, array, 0, array.Length);
        return array;
    }

    /// <summary>
    /// 选择一个数组的前面的几个数据信息。
    /// </summary>
    /// <param name="value">数组</param>
    /// <param name="length">数据的长度</param>
    /// <returns>新的数组</returns>
    public static T[] SelectBegin<T>(this T[] value, int length)
    {
        if (length == 0)
        {
            return [];
        }

        var array = new T[Math.Min(value.Length, length)];
        if (array.Length != 0)
        {
            Array.Copy(value, 0, array, 0, array.Length);
        }
        return array;
    }

    /// <summary>
    /// 选择一个数组的后面的几个数据信息。
    /// </summary>
    /// <param name="value">数组</param>
    /// <param name="length">数据的长度</param>
    /// <returns>新的数组信息</returns>
    public static T[] SelectLast<T>(this T[] value, int length)
    {
        if (length == 0)
        {
            return [];
        }

        var array = new T[Math.Min(value.Length, length)];
        Array.Copy(value, value.Length - length, array, 0, array.Length);
        return array;
    }

    /// <summary>
    /// 拷贝当前的实例数组，是基于引用层的浅拷贝，如果类型为值类型，那就是深度拷贝，如果类型为引用类型，就是浅拷贝。
    /// </summary>
    /// <typeparam name="T">类型对象</typeparam>
    /// <param name="value">数组对象</param>
    /// <returns>拷贝的结果内容</returns>
    public static T[] CopyArray<T>(this T[] value)
    {
        var array = new T[value.Length];
        Array.Copy(value, array, value.Length);
        return array;
    }

    /// <summary>
    /// 将byte数组按照双字节进行反转，如果为单数的情况，则自动补齐。
    /// </summary>
    /// <remarks>
    /// 例如传入的字节数据是 01 02 03 04, 那么反转后就是 02 01 04 03
    /// </remarks>
    /// <param name="inBytes">输入的字节信息</param>
    /// <returns>反转后的数据</returns>
    public static byte[] ReverseByWord(this byte[] inBytes)
    {
        if (inBytes.Length == 0)
        {
            return [];
        }

        var array = CollectionUtils.ExpandToEvenLength(inBytes.CopyArray());
        for (var i = 0; i < array.Length / 2; i++)
        {
            (array[i * 2 + 1], array[i * 2]) = (array[i * 2], array[i * 2 + 1]);
        }
        return array;
    }

    /// <summary>
    /// 从字节构建一个ASCII格式的数据内容。
    /// </summary>
    /// <param name="value">数据</param>
    /// <returns>ASCII格式的字节数组</returns>
    public static byte[] BuildAsciiBytesFrom(byte value) => Encoding.ASCII.GetBytes(value.ToString("X2"));

    /// <summary>
    /// 从short构建一个ASCII格式的数据内容。
    /// </summary>
    /// <param name="value">数据</param>
    /// <returns>ASCII格式的字节数组</returns>
    public static byte[] BuildAsciiBytesFrom(short value) => Encoding.ASCII.GetBytes(value.ToString("X4"));

    /// <summary>
    /// 从ushort构建一个ASCII格式的数据内容。
    /// </summary>
    /// <param name="value">数据</param>
    /// <returns>ASCII格式的字节数组</returns>
    public static byte[] BuildAsciiBytesFrom(ushort value) => Encoding.ASCII.GetBytes(value.ToString("X4"));

    /// <summary>
    /// 从uint构建一个ASCII格式的数据内容。
    /// </summary>
    /// <param name="value">数据</param>
    /// <returns>ASCII格式的字节数组</returns>
    public static byte[] BuildAsciiBytesFrom(uint value) => Encoding.ASCII.GetBytes(value.ToString("X8"));

    /// <summary>
    /// 从字节数组构建一个ASCII格式的数据内容。
    /// </summary>
    /// <param name="value">字节信息</param>
    /// <returns>ASCII格式的地址</returns>
    public static byte[] BuildAsciiBytesFrom(byte[] value)
    {
        var array = new byte[value.Length * 2];
        for (var i = 0; i < value.Length; i++)
        {
            BuildAsciiBytesFrom(value[i]).CopyTo(array, 2 * i);
        }
        return array;
    }

    /// <summary>
    /// 获取byte数据类型的第offset位，是否为True。
    /// </summary>
    /// <param name="value">byte数值</param>
    /// <param name="offset">索引位置</param>
    /// <returns>结果</returns>
    private static bool BoolOnByteIndex(byte value, int offset)
    {
        var dataByBitIndex = GetDataByBitIndex(offset);
        return (value & dataByBitIndex) == dataByBitIndex;
    }

    private static byte GetDataByBitIndex(int offset)
    {
        return offset switch
        {
            0 => 1,
            1 => 2,
            2 => 4,
            3 => 8,
            4 => 16,
            5 => 32,
            6 => 64,
            7 => 128,
            _ => 0,
        };
    }
}
