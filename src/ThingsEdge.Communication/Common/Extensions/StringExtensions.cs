using System.Text.RegularExpressions;

namespace ThingsEdge.Communication.Common.Extensions;

internal static class StringExtensions
{
    /// <summary>
    /// 将16进制的字符串转化成Byte数据，将检测每2个字符转化，也就是说，中间可以是任意字符。
    /// </summary>
    /// <param name="str">十六进制的字符串，中间可以是任意的分隔符</param>
    /// <returns>转换后的字节数组</returns>
    /// <remarks>参数举例：AA 01 34 A8</remarks>
    public static byte[] ToHexBytes(this string str)
    {
        using var memoryStream = new MemoryStream();
        for (var i = 0; i < str.Length; i++)
        {
            if (i + 1 < str.Length && GetHexCharIndex(str[i]) >= 0 && GetHexCharIndex(str[i + 1]) >= 0)
            {
                memoryStream.WriteByte((byte)(GetHexCharIndex(str[i]) * 16 + GetHexCharIndex(str[i + 1])));
                i++;
            }
        }
        var result = memoryStream.ToArray();
        return result;

        static int GetHexCharIndex(char ch)
        {
            return ch switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                '3' => 3,
                '4' => 4,
                '5' => 5,
                '6' => 6,
                '7' => 7,
                '8' => 8,
                '9' => 9,
                'A' or 'a' => 10,
                'B' or 'b' => 11,
                'C' or 'c' => 12,
                'D' or 'd' => 13,
                'E' or 'e' => 14,
                'F' or 'f' => 15,
                _ => -1,
            };
        }
    }

    /// <summary>
    /// 根据英文小数点进行切割字符串，去除空白的字符。
    /// </summary>
    /// <param name="str">字符串本身</param>
    /// <returns>切割好的字符串数组，例如输入 "100.5"，返回 "100", "5"</returns>
    public static string[] SplitDot(this string str)
    {
        return str.Split('.', StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// 判断一个字符串是否是指定的字符串开头，并且紧跟着指定字符串的后一位是 char，如果是，返回 true，反之，返回 false。
    /// </summary>
    /// <param name="str">等待判断的字符串</param>
    /// <param name="value">字符串类型</param>
    /// <returns>是否指定的地址起始</returns>
    public static bool StartsWithAndNextIsNumber(this string str, string value)
    {
        return str.StartsWith(value, StringComparison.InvariantCultureIgnoreCase) && char.IsNumber(str[value.Length]);
    }

    /// <summary>
    /// 字符串是否以包含任何一个指定值开头。
    /// </summary>
    /// <param name="str">字符串</param>
    /// <param name="values">指定值集合</param>
    /// <returns></returns>
    public static bool StartsWith(this string str, string[] values)
    {
        if (values.Length == 0)
        {
            return false;
        }

        for (var i = 0; i < values.Length; i++)
        {
            if (str.StartsWith(values[i]))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 字符串中是否以包含任何一个指定值。
    /// </summary>
    /// <param name="str">字符串</param>
    /// <param name="values">指定值集合</param>
    /// <returns></returns>
    public static bool Contains(this string str, string[] values)
    {
        if (values.Length == 0)
        {
            return false;
        }

        for (var i = 0; i < values.Length; i++)
        {
            if (str.Contains(values[i]))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 获取地址信息的位索引，在地址最后一个小数点的位置
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <returns>位索引的位置</returns>
    public static int GetBitIndexInformation(ref string address)
    {
        var result = 0;
        var num = address.LastIndexOf('.');
        if (num > 0 && num < address.Length - 1)
        {
            var text = address[(num + 1)..];
            result = !text.Contains(["A", "B", "C", "D", "E", "F"]) ? Convert.ToInt32(text) : Convert.ToInt32(text, 16);
            address = address[..num];
        }
        return result;
    }

    /// <summary>
    /// 解析地址的起始地址的方法，比如你的地址是 A[1] , 那么将会返回 1，地址修改为 A，如果不存在起始地址，那么就不修改地址，返回 -1。
    /// </summary>
    /// <param name="address">复杂的地址格式，比如：A[0] </param>
    /// <returns>如果存在，就起始位置，不存在就返回 -1</returns>
    public static int ExtractStartIndex(ref string address)
    {
        try
        {
            var match = Regex.Match(address, "\\[[0-9]+\\]$");
            if (!match.Success)
            {
                return -1;
            }
            var value = match.Value[1..^1];
            var result = Convert.ToInt32(value);
            address = address.Remove(address.Length - match.Value.Length);
            return result;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// 判断当前的字符串表示的地址，是否以索引为结束
    /// </summary>
    /// <param name="address">PLC的字符串地址信息</param>
    /// <returns>是否以索引结束</returns>
    public static bool IsAddressEndWithIndex(string address)
    {
        return Regex.IsMatch(address, "\\[[0-9]+\\]$");
    }
}
