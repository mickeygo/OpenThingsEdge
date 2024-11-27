namespace ThingsEdge.Communication.Common.Extensions;

/// <summary>
/// 数据扩展的辅助类方法
/// </summary>
internal static class ByteExtensions
{
    /// <summary>
    /// 将原始的byte数组转换成ascii格式的byte数组。
    /// </summary>
    /// <param name="bytes">等待转换的byte数组</param>
    /// <returns>转换后的数组</returns>
    public static byte[] ToAsciiBytes(this byte[] bytes)
    {
        return Encoding.ASCII.GetBytes(ToHexString(bytes));
    }

    /// <summary>
    /// 将字节数组显示为ASCII格式的字符串，当遇到0x20以下及0x7E以上的不可见字符时，使用十六进制的数据显示。
    /// </summary>
    /// <param name="bytes">字节数组信息</param>
    /// <returns>ASCII格式的字符串信息</returns>
    public static string ToAsciiString(this byte[] bytes)
    {
        var stringBuilder = new StringBuilder();
        for (var i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] < 32 || bytes[i] > 126)
            {
                stringBuilder.Append($"\\{bytes[i]:X2}");
            }
            else
            {
                stringBuilder.Append((char)bytes[i]);
            }
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 字节数据转化成16进制表示的字符串。
    /// </summary>
    /// <param name="bytes">字节数组</param>
    /// <returns>返回的字符串</returns>
    public static string ToHexString(this byte[]? bytes)
    {
        if (bytes == null)
        {
            return string.Empty;
        }

        return ToHexString(bytes, '\0');
    }

    /// <summary>
    /// 字节数据转化成16进制表示的字符串。
    /// </summary>
    /// <param name="bytes">字节数组</param>
    /// <param name="segment">分割符</param>
    /// <returns>返回的字符串</returns>
    public static string ToHexString(this byte[]? bytes, char segment)
    {
        if (bytes == null)
        {
            return string.Empty;
        }

        return ToHexString(bytes, segment, 0);
    }

    /// <summary>
    /// 字节数据转化成16进制表示的字符串。
    /// </summary>
    /// <param name="bytes">字节数组</param>
    /// <param name="segment">分割符，如果设置为0，则没有分隔符信息</param>
    /// <param name="newLineCount">每隔指定数量的时候进行换行，如果小于等于0，则不进行分行显示</param>
    /// <returns>返回的字符串</returns>
    public static string ToHexString(this byte[] bytes, char segment, int newLineCount)
    {
        var stringBuilder = new StringBuilder();
        var num = 0L;
        foreach (var b in bytes)
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
    /// <param name="bytes">原先的字节数组</param>
    /// <returns>转换后的bool数组</returns>
    public static bool[] ToBoolArray(this byte[] bytes)
    {
        return ToBoolArray(bytes, bytes.Length * 8);
    }

    /// <summary>
    /// 从Byte数组中提取位数组，length代表位数，例如数组 03 A1 长度10转为 [1100 0000 10]。
    /// </summary>
    /// <param name="bytes">原先的字节数组</param>
    /// <param name="length">想要转换的长度，如果超出自动会缩小到数组最大长度</param>
    /// <returns>转换后的bool数组</returns>
    public static bool[] ToBoolArray(this byte[] bytes, int length)
    {
        if (length > bytes.Length * 8)
        {
            length = bytes.Length * 8;
        }

        var array = new bool[length];
        for (var i = 0; i < length; i++)
        {
            array[i] = BoolOnByteIndex(bytes[i / 8], i % 8);
        }
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
    /// 将byte数组按照双字节进行反转，如果为单数的情况，则自动补齐。
    /// </summary>
    /// <remarks>
    /// 例如传入的字节数据是 01 02 03 04, 那么反转后就是 02 01 04 03
    /// </remarks>
    /// <param name="bytes">输入的字节信息</param>
    /// <returns>反转后的数据</returns>
    public static byte[] ReverseByWord(this byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return [];
        }

        var array = CollectionUtils.ExpandToEvenLength(CollectionUtils.CopyArray(bytes));
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
