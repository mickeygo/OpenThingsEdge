using System.IO.Ports;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Exceptions;

namespace ThingsEdge.Communication.Common;

/// <summary>
/// 扩展的辅助类方法
/// </summary>
public static class DataExtensions
{
    /// <inheritdoc cref="SoftBasic.ByteToHexString(byte[])" />
    public static string ToHexString(this byte[] InBytes)
    {
        return SoftBasic.ByteToHexString(InBytes);
    }

    /// <inheritdoc cref="SoftBasic.ByteToHexString(byte[],char)" />
    public static string ToHexString(this byte[] InBytes, char segment)
    {
        return SoftBasic.ByteToHexString(InBytes, segment);
    }

    /// <inheritdoc cref="SoftBasic.ByteToHexString(byte[],char,int,string)" />
    public static string ToHexString(this byte[] InBytes, char segment, int newLineCount, string format = "{0:X2}")
    {
        return SoftBasic.ByteToHexString(InBytes, segment, newLineCount, format);
    }

    /// <inheritdoc cref="SoftBasic.HexStringToBytes(string)" />
    public static byte[] ToHexBytes(this string value)
    {
        return SoftBasic.HexStringToBytes(value);
    }

    /// <inheritdoc cref="SoftBasic.BoolArrayToByte(bool[])" />
    public static byte[] ToByteArray(this bool[] array)
    {
        return SoftBasic.BoolArrayToByte(array);
    }

    /// <inheritdoc cref="SoftBasic.ByteToBoolArray(byte[])" />
    public static bool[] ToBoolArray(this byte[] InBytes)
    {
        return SoftBasic.ByteToBoolArray(InBytes);
    }

    /// <summary>
    /// 获取Byte数组的第 boolIndex 偏移的bool值，这个偏移值可以为 10，就是第 1 个字节的 第3位。
    /// </summary>
    /// <param name="bytes">字节数组信息</param>
    /// <param name="boolIndex">指定字节的位偏移</param>
    /// <returns>bool值</returns>
    public static bool GetBoolByIndex(this byte[] bytes, int boolIndex)
    {
        return SoftBasic.BoolOnByteIndex(bytes[boolIndex / 8], boolIndex % 8);
    }

    /// <summary>
    /// 获取Byte的第 boolIndex 偏移的bool值，比如3，就是第4位。
    /// </summary>
    /// <param name="byt">字节信息</param>
    /// <param name="boolIndex">指定字节的位偏移</param>
    /// <returns>bool值</returns>
    public static bool GetBoolByIndex(this byte byt, int boolIndex)
    {
        return SoftBasic.BoolOnByteIndex(byt, boolIndex % 8);
    }

    /// <summary>
    /// 设置Byte的第 boolIndex 位的bool值，可以强制为 true 或是 false, 不影响其他的位。
    /// </summary>
    /// <param name="byt">字节信息</param>
    /// <param name="boolIndex">指定字节的位偏移</param>
    /// <param name="value">bool的值</param>
    /// <returns>修改之后的byte值</returns>
    public static byte SetBoolByIndex(this byte byt, int boolIndex, bool value)
    {
        return SoftBasic.SetBoolOnByteIndex(byt, boolIndex, value);
    }

    /// <inheritdoc cref="SoftBasic.ArrayRemoveDouble" />
    public static T[] RemoveDouble<T>(this T[] value, int leftLength, int rightLength)
    {
        return SoftBasic.ArrayRemoveDouble(value, leftLength, rightLength);
    }

    /// <inheritdoc cref="SoftBasic.ArrayRemoveBegin" />
    public static T[] RemoveBegin<T>(this T[] value, int length)
    {
        return SoftBasic.ArrayRemoveBegin(value, length);
    }

    /// <inheritdoc cref="SoftBasic.ArrayRemoveLast" />
    public static T[] RemoveLast<T>(this T[] value, int length)
    {
        return SoftBasic.ArrayRemoveLast(value, length);
    }

    /// <inheritdoc cref="SoftBasic.ArraySelectMiddle" />
    public static T[] SelectMiddle<T>(this T[] value, int index, int length)
    {
        return SoftBasic.ArraySelectMiddle(value, index, length);
    }

    /// <inheritdoc cref="SoftBasic.ArraySelectBegin" />
    public static T[] SelectBegin<T>(this T[] value, int length)
    {
        return SoftBasic.ArraySelectBegin(value, length);
    }

    /// <inheritdoc cref="SoftBasic.ArraySelectLast" />
    public static T[] SelectLast<T>(this T[] value, int length)
    {
        return SoftBasic.ArraySelectLast(value, length);
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
    /// 将字符串数组转换为实际的数据数组。例如字符串格式[1,2,3,4,5]，可以转成实际的数组对象。
    /// </summary>
    /// <typeparam name="T">类型对象</typeparam>
    /// <param name="value">字符串数据</param>
    /// <param name="selector">转换方法</param>
    /// <returns>实际的数组</returns>
    public static T[] ToStringArray<T>(this string value, Func<string, T> selector)
    {
        if (value.Contains('['))
        {
            value = value.Replace("[", "");
        }
        if (value.Contains(']'))
        {
            value = value.Replace("]", "");
        }
        var source = value.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries);
        return source.Select(selector).ToArray();
    }

    /// <summary>
    /// 将字符串数组转换为实际的数据数组。支持byte,sbyte,bool,short,ushort,int,uint,long,ulong,float,double，使用默认的十进制，例如字符串格式[1,2,3,4,5]，可以转成实际的数组对象。
    /// </summary>
    /// <typeparam name="T">类型对象</typeparam>
    /// <param name="value">字符串数据</param>
    /// <returns>实际的数组</returns>
    /// <exception cref="Exception"></exception>
    public static T[] ToStringArray<T>(this string value)
    {
        var typeFromHandle = typeof(T);
        if (typeFromHandle == typeof(byte))
        {
            return (T[])(object)value.ToStringArray(byte.Parse);
        }
        if (typeFromHandle == typeof(sbyte))
        {
            return (T[])(object)value.ToStringArray(sbyte.Parse);
        }
        if (typeFromHandle == typeof(bool))
        {
            return (T[])(object)value.ToStringArray(bool.Parse);
        }
        if (typeFromHandle == typeof(short))
        {
            return (T[])(object)value.ToStringArray(short.Parse);
        }
        if (typeFromHandle == typeof(ushort))
        {
            return (T[])(object)value.ToStringArray(ushort.Parse);
        }
        if (typeFromHandle == typeof(int))
        {
            return (T[])(object)value.ToStringArray(int.Parse);
        }
        if (typeFromHandle == typeof(uint))
        {
            return (T[])(object)value.ToStringArray(uint.Parse);
        }
        if (typeFromHandle == typeof(long))
        {
            return (T[])(object)value.ToStringArray(long.Parse);
        }
        if (typeFromHandle == typeof(ulong))
        {
            return (T[])(object)value.ToStringArray(ulong.Parse);
        }
        if (typeFromHandle == typeof(float))
        {
            return (T[])(object)value.ToStringArray(float.Parse);
        }
        if (typeFromHandle == typeof(double))
        {
            return (T[])(object)value.ToStringArray(double.Parse);
        }
        if (typeFromHandle == typeof(DateTime))
        {
            return (T[])(object)value.ToStringArray(DateTime.Parse);
        }
        if (typeFromHandle == typeof(Guid))
        {
            return (T[])(object)value.ToStringArray(Guid.Parse);
        }
        if (typeFromHandle == typeof(string))
        {
            return (T[])(object)value.ToStringArray((m) => m);
        }

        throw new CommunicationException("use ToArray<T>(Func<string,T>) method instead");
    }

    /// <summary>
    /// 启动接收数据，需要传入回调方法，传递对象。
    /// </summary>
    /// <param name="socket">socket对象</param>
    /// <param name="callback">回调方法</param>
    /// <param name="obj">数据对象</param>
    /// <returns>是否启动成功</returns>
    public static OperateResult BeginReceiveResult(this Socket socket, AsyncCallback callback, object obj)
    {
        try
        {
            socket.BeginReceive([], 0, 0, SocketFlags.None, callback, obj);
            return OperateResult.CreateSuccessResult();
        }
        catch (Exception ex)
        {
            socket?.Close();
            return new OperateResult(ex.Message);
        }
    }

    /// <summary>
    /// 启动接收数据，需要传入回调方法，传递对象默认为socket本身。
    /// </summary>
    /// <param name="socket">socket对象</param>
    /// <param name="callback">回调方法</param>
    /// <returns>是否启动成功</returns>
    public static OperateResult BeginReceiveResult(this Socket socket, AsyncCallback callback)
    {
        return socket.BeginReceiveResult(callback, socket);
    }

    /// <summary>
    /// 结束挂起的异步读取，返回读取的字节数，如果成功的情况。
    /// </summary>
    /// <param name="socket">socket对象</param>
    /// <param name="ar">回调方法</param>
    /// <returns>是否启动成功</returns>
    public static OperateResult<int> EndReceiveResult(this Socket socket, IAsyncResult ar)
    {
        try
        {
            return OperateResult.CreateSuccessResult(socket.EndReceive(ar));
        }
        catch (Exception ex)
        {
            socket?.Close();
            return new OperateResult<int>(ex.Message);
        }
    }

    /// <summary>
    /// 根据英文小数点进行切割字符串，去除空白的字符。
    /// </summary>
    /// <param name="str">字符串本身</param>
    /// <returns>切割好的字符串数组，例如输入 "100.5"，返回 "100", "5"</returns>
    public static string[] SplitDot(this string str)
    {
        return str.Split(['.'], StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// 设置套接字的活动时间和活动间歇时间，此值会设置到socket低级别的控制中，传入值如果为负数，则表示不使用 KeepAlive 功能。
    /// </summary>
    /// <param name="socket">套接字对象</param>
    /// <param name="keepAliveTime">保持活动时间</param>
    /// <param name="keepAliveInterval">保持活动的间歇时间</param>
    /// <returns>返回获取的参数的字节</returns>
    public static int SetKeepAlive(this Socket socket, int keepAliveTime, int keepAliveInterval)
    {
        var array = new byte[12];
        BitConverter.GetBytes(keepAliveTime >= 0 ? 1 : 0).CopyTo(array, 0);
        BitConverter.GetBytes(keepAliveTime).CopyTo(array, 4);
        BitConverter.GetBytes(keepAliveInterval).CopyTo(array, 8);
        try
        {
            return socket.IOControl(IOControlCode.KeepAliveValues, array, null);
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// 使用格式化的串口参数信息来初始化串口的参数，举例：9600-8-N-1，分别表示波特率，数据位，奇偶校验，停止位，当然也可以携带串口名称，例如：COM3-9600-8-N-1，linux环境也是支持的。
    /// </summary>
    /// <remarks>
    /// 其中奇偶校验的字母可选，N:无校验，O：奇校验，E:偶校验，停止位可选 0, 1, 2, 1.5 四种选项。
    /// </remarks>
    /// <param name="serialPort">串口对象信息</param>
    /// <param name="format">格式化的参数内容，例如：9600-8-N-1</param>
    public static void IniSerialByFormatString(this SerialPort serialPort, string format)
    {
        var array = format.Split(['-', ';'], StringSplitOptions.RemoveEmptyEntries);
        if (array.Length != 0)
        {
            var num = 0;
            if (!Regex.IsMatch(array[0], "^[0-9]+$"))
            {
                serialPort.PortName = array[0];
                num = 1;
            }
            if (num < array.Length)
            {
                serialPort.BaudRate = Convert.ToInt32(array[num++]);
            }
            if (num < array.Length)
            {
                serialPort.DataBits = Convert.ToInt32(array[num++]);
            }
            if (num < array.Length)
            {
                serialPort.Parity = array[num++].ToUpper() switch
                {
                    "E" => Parity.Even,
                    "O" => Parity.Odd,
                    "N" => Parity.None,
                    _ => Parity.Space,
                };
            }
            if (num < array.Length)
            {
                serialPort.StopBits = array[num++] switch
                {
                    "0" => StopBits.None,
                    "2" => StopBits.Two,
                    "1" => StopBits.One,
                    _ => StopBits.OnePointFive,
                };
            }
        }
    }

    /// <summary>
    /// 将一个串口对象的基本配置参数进行格式化字符串，例如 COM3-9600-8-N-1。
    /// </summary>
    /// <remarks>
    /// 其中奇偶校验的字母可选，N:无校验，O：奇校验，E:偶校验，停止位可选 0, 1, 2, 1.5 四种选项。
    /// </remarks>
    /// <param name="serialPort">串口对象信息</param>
    /// <returns>串口对的格式化字符串信息</returns>
    public static string ToFormatString(this SerialPort serialPort)
    {
        return CommHelper.ToFormatString(serialPort.PortName, serialPort.BaudRate, serialPort.DataBits, serialPort.Parity, serialPort.StopBits);
    }

    /// <summary>
    /// 根据指定的字节长度信息，获取到随机的字节信息。
    /// </summary>
    /// <param name="random">随机数对象</param>
    /// <param name="length">字节的长度信息</param>
    /// <returns>原始字节数组</returns>
    public static byte[] GetBytes(this Random random, int length)
    {
        var array = new byte[length];
        random.NextBytes(array);
        return array;
    }

    /// <inheritdoc cref="SoftBasic.BytesReverseByWord(byte[])" />
    public static byte[] ReverseByWord(this byte[] inBytes)
    {
        return SoftBasic.BytesReverseByWord(inBytes);
    }

    /// <summary>
    /// 判断一个地址是否是指定的字符串开头，并且后面跟着数字，如果是，返回 true，反之，返回 false。
    /// </summary>
    /// <param name="address">等待判断的地址</param>
    /// <param name="code">地址类型</param>
    /// <returns>是否指定的地址起始</returns>
    public static bool StartsWithAndNumber(this string address, string code)
    {
        if (address.StartsWith(code, StringComparison.InvariantCultureIgnoreCase) && char.IsNumber(address[code.Length]))
        {
            return true;
        }
        return false;
    }

    /// <inheritdoc cref="M:System.String.StartsWith(System.String)" />
    public static bool StartsWith(this string address, string[] code)
    {
        if (code == null)
        {
            return false;
        }
        for (var i = 0; i < code.Length; i++)
        {
            if (address.StartsWith(code[i]))
            {
                return true;
            }
        }
        return false;
    }

    /// <inheritdoc cref="M:System.String.EndsWith(System.String)" />
    public static bool EndsWith(this string str, string[] value)
    {
        if (value == null)
        {
            return false;
        }

        for (var i = 0; i < value.Length; i++)
        {
            if (str.EndsWith(value[i], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    /// <inheritdoc cref="M:System.String.Contains(System.String)" />
    public static bool Contains(this string str, string[] value)
    {
        if (value == null)
        {
            return false;
        }
        for (var i = 0; i < value.Length; i++)
        {
            if (str.Contains(value[i]))
            {
                return true;
            }
        }
        return false;
    }
}
