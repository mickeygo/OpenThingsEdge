using System.Text.RegularExpressions;

namespace ThingsEdge.Communication.Core;

/// <summary>
/// Communication的一些静态辅助方法。
/// </summary>
public static class CommHelper
{
    /// <summary>
    /// 解析地址的附加参数方法，比如你的地址是s=100;D100，可以提取出"s"的值的同时，修改地址本身，如果"s"不存在的话，返回给定的默认值。
    /// </summary>
    /// <param name="address">复杂的地址格式，比如：s=100;D100</param>
    /// <param name="paraName">等待提取的参数名称</param>
    /// <param name="defaultValue">如果提取的参数信息不存在，返回的默认值信息</param>
    /// <returns>解析后的新的数据值或是默认的给定的数据值</returns>
    public static int ExtractParameter(ref string address, string paraName, int defaultValue)
    {
        var operateResult = ExtractParameter(ref address, paraName);
        return operateResult.IsSuccess ? operateResult.Content : defaultValue;
    }

    /// <summary>
    /// 解析地址的附加参数方法，比如你的地址是s=100;D100，可以提取出"s"的值的同时，修改地址本身，如果"s"不存在的话，返回错误的消息内容。
    /// </summary>
    /// <param name="address">复杂的地址格式，比如：s=100;D100</param>
    /// <param name="paraName">等待提取的参数名称</param>
    /// <returns>解析后的参数结果内容</returns>
    public static OperateResult<int> ExtractParameter(ref string address, string paraName)
    {
        try
        {
            var match = Regex.Match(address, paraName + "=[0-9A-Fa-fxX]+;", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return new OperateResult<int>("Address [" + address + "] can't find [" + paraName + "] Parameters. for example : " + paraName + "=1;100");
            }
            var text = match.Value.Substring(paraName.Length + 1, match.Value.Length - paraName.Length - 2);
            var value = text.StartsWith("0x") || text.StartsWith("0X")
                ? Convert.ToInt32(text[2..], 16)
                : text.StartsWith('0') ? Convert.ToInt32(text, 8) : Convert.ToInt32(text);
            address = address.Replace(match.Value, "");
            return OperateResult.CreateSuccessResult(value);
        }
        catch (Exception ex)
        {
            return new OperateResult<int>("Address [" + address + "] Get [" + paraName + "] Parameters failed: " + ex.Message);
        }
    }

    /// <summary>
    /// 解析地址的附加<see cref="DataFormat" />参数方法，比如你的地址是format=ABCD;D100，可以提取出"format"的值的同时，修改地址本身，
    /// 如果"format"不存在的话，返回默认的<see cref="IByteTransform" />对象。
    /// </summary>
    /// <param name="address">复杂的地址格式，比如：format=ABCD;D100</param>
    /// <param name="defaultTransform">默认的数据转换信息</param>
    /// <returns>解析后的参数结果内容</returns>
    public static IByteTransform ExtractTransformParameter(ref string address, IByteTransform defaultTransform)
    {
        try
        {
            var text = "format";
            var match = Regex.Match(address, text + "=(ABCD|BADC|DCBA|CDAB);", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return defaultTransform;
            }
            var text2 = match.Value.Substring(text.Length + 1, match.Value.Length - text.Length - 2);
            var dataFormat = text2.ToUpper() switch
            {
                "ABCD" => DataFormat.ABCD,
                "BADC" => DataFormat.BADC,
                "DCBA" => DataFormat.DCBA,
                "CDAB" => DataFormat.CDAB,
                _ => defaultTransform.DataFormat,
            };
            address = address.Replace(match.Value, "");
            if (dataFormat != defaultTransform.DataFormat)
            {
                return defaultTransform.CreateByDateFormat(dataFormat);
            }
            return defaultTransform;
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// 根据当前的位偏移地址及读取位长度信息，计算出实际的字节索引，字节数，字节位偏移
    /// </summary>
    /// <param name="addressStart">起始地址</param>
    /// <param name="length">读取的长度</param>
    /// <param name="newStart">返回的新的字节的索引，仍然按照位单位</param>
    /// <param name="byteLength">字节长度</param>
    /// <param name="offset">当前偏移的信息</param>
    public static void CalculateStartBitIndexAndLength(int addressStart, ushort length, out int newStart, out ushort byteLength, out int offset)
    {
        byteLength = (ushort)((addressStart + length - 1) / 8 - addressStart / 8 + 1);
        offset = addressStart % 8;
        newStart = addressStart - offset;
    }

    /// <summary>
    /// 根据字符串内容，获取当前的位索引地址，例如输入 6,返回6，输入15，返回15，输入B，返回11
    /// </summary>
    /// <param name="bit">位字符串</param>
    /// <returns>结束数据</returns>
    public static int CalculateBitStartIndex(string bit)
    {
        if (Regex.IsMatch(bit, "[ABCDEF]", RegexOptions.IgnoreCase))
        {
            return Convert.ToInt32(bit, 16);
        }
        return Convert.ToInt32(bit);
    }

    /// <summary>
    /// 将一个一维数组中的所有数据按照行列信息拷贝到二维数组里，返回当前的二维数组
    /// </summary>
    /// <typeparam name="T">数组的类型对象</typeparam>
    /// <param name="array">一维数组信息</param>
    /// <param name="row">行</param>
    /// <param name="col">列</param>
    public static T[,] CreateTwoArrayFromOneArray<T>(T[] array, int row, int col)
    {
        var array2 = new T[row, col];
        var num = 0;
        for (var i = 0; i < row; i++)
        {
            for (var j = 0; j < col; j++)
            {
                array2[i, j] = array[num];
                num++;
            }
        }
        return array2;
    }

    /// <summary>
    /// 根据位偏移的地址，长度信息，计算出实际的地址占用长度
    /// </summary>
    /// <param name="address">偏移地址</param>
    /// <param name="length">长度信息</param>
    /// <param name="hex">地址的进制信息，一般为8 或是 16</param>
    /// <returns>占用的地址长度信息</returns>
    public static int CalculateOccupyLength(int address, int length, int hex = 8)
    {
        return (address + length - 1) / hex - address / hex + 1;
    }

    /// <summary>
    /// 按照位为单位从设备中批量读取bool数组，如果地址中包含了小数点，则使用字的方式读取数据，然后解析出位数据。
    /// </summary>
    /// <param name="device">设备的通信对象</param>
    /// <param name="address">地址信息</param>
    /// <param name="length">读取的位长度信息</param>
    /// <param name="addressLength">单位地址的占位长度信息</param>
    /// <param name="reverseByWord">是否根据字进行反转操作</param>
    /// <returns>bool数组的结果对象</returns>
    public static async Task<OperateResult<bool[]>> ReadBoolAsync(IReadWriteNet device, string address, ushort length, int addressLength = 16, bool reverseByWord = false)
    {
        if (address.IndexOf('.') > 0)
        {
            var addressSplits = address.SplitDot();
            int bitIndex;
            try
            {
                bitIndex = CalculateBitStartIndex(addressSplits[1]);
            }
            catch (Exception ex2)
            {
                var ex = ex2;
                return new OperateResult<bool[]>("Bit Index format wrong, " + ex.Message);
            }

            var read = await device.ReadAsync(addressSplits[0], (ushort)((length + bitIndex + addressLength - 1) / addressLength)).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(read);
            }

            if (reverseByWord)
            {
                return OperateResult.CreateSuccessResult(read.Content!.ReverseByWord().ToBoolArray().SelectMiddle(bitIndex, length));
            }
            return OperateResult.CreateSuccessResult(read.Content!.ToBoolArray().SelectMiddle(bitIndex, length));
        }
        return await device.ReadBoolAsync(address, length).ConfigureAwait(false);
    }
}
