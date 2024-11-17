using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Profinet.Siemens;

/// <summary>
/// Contains the methods to convert between <see cref="DateTime" /> and S7 representation of datetime values.
/// </summary>
/// <remarks>
/// 这部分的代码参考了另一个s7的库，感谢原作者，此处贴出出处，遵循 MIT 协议
/// https://github.com/S7NetPlus/s7netplus
/// </remarks>
public sealed class SiemensDateTime
{
    /// <summary>
    /// The minimum <see cref="DateTime" /> value supported by the specification.
    /// </summary>
    public static readonly DateTime SpecMinimumDateTime = new(1990, 1, 1);

    /// <summary>
    /// The maximum <see cref="DateTime" /> value supported by the specification.
    /// </summary>
    public static readonly DateTime SpecMaximumDateTime = new(2089, 12, 31, 23, 59, 59, 999);

    /// <summary>
    /// Parses a <see cref="DateTime" /> value from bytes.
    /// </summary>
    /// <param name="bytes">Input bytes read from PLC.</param>
    /// <returns>A <see cref="DateTime" /> object representing the value read from PLC.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the length of
    ///   <paramref name="bytes" /> is not 8 or any value in <paramref name="bytes" />
    ///   is outside the valid range of values.</exception>
    public static OperateResult<DateTime> FromByteArray(byte[] bytes)
    {
        try
        {
            return OperateResult.CreateSuccessResult(FromByteArrayImpl(bytes));
        }
        catch (Exception ex)
        {
            return new OperateResult<DateTime>("Prase DateTime failed: " + ex.Message);
        }
    }

    /// <summary>
    /// 从西门子的原始字节数据中，提取出DTL格式的时间信息
    /// </summary>
    /// <param name="byteTransform">西门子的字节变换对象</param>
    /// <param name="buffer">原始字节数据</param>
    /// <param name="index">字节偏移索引</param>
    /// <returns>时间信息</returns>
    public static OperateResult<DateTime> GetDTLTime(IByteTransform byteTransform, byte[] buffer, int index)
    {
        try
        {
            int year = byteTransform.TransInt16(buffer, index);
            int month = buffer[index + 2];
            int day = buffer[index + 3];
            int hour = buffer[index + 5];
            int minute = buffer[index + 6];
            int second = buffer[index + 7];
            var millisecond = byteTransform.TransInt32(buffer, index + 8) / 1000 / 1000;
            return OperateResult.CreateSuccessResult(new DateTime(year, month, day, hour, minute, second, millisecond));
        }
        catch (Exception ex)
        {
            return new OperateResult<DateTime>("GetDTLTime failed: " + ex.Message);
        }
    }

    /// <summary>
    /// 将时间数据转换为西门子的DTL格式的时间数据
    /// </summary>
    /// <param name="byteTransform">西门子的字节变换对象</param>
    /// <param name="dateTime">指定的时间信息</param>
    /// <returns>原始字节数据信息</returns>
    public static byte[] GetBytesFromDTLTime(IByteTransform byteTransform, DateTime dateTime)
    {
        var array = new byte[12];
        byteTransform.TransByte((short)dateTime.Year).CopyTo(array, 0);
        array[2] = (byte)dateTime.Month;
        array[3] = (byte)dateTime.Day;
        array[4] = 5;
        array[5] = (byte)dateTime.Hour;
        array[6] = (byte)dateTime.Minute;
        array[7] = (byte)dateTime.Second;
        byteTransform.TransByte(dateTime.Millisecond * 1000 * 1000).CopyTo(array, 8);
        return array;
    }

    /// <summary>
    /// Parses an array of <see cref="DateTime" /> values from bytes.
    /// </summary>
    /// <param name="bytes">Input bytes read from PLC.</param>
    /// <returns>An array of <see cref="DateTime" /> objects representing the values read from PLC.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the length of
    ///   <paramref name="bytes" /> is not a multiple of 8 or any value in
    ///   <paramref name="bytes" /> is outside the valid range of values.</exception>
    public static DateTime[] ToArray(byte[] bytes)
    {
        if (bytes.Length % 8 != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bytes), bytes.Length, $"Parsing an array of DateTime requires a multiple of 8 bytes of input data, input data is '{bytes.Length}' long.");
        }

        var num = bytes.Length / 8;
        var array = new DateTime[bytes.Length / 8];
        for (var i = 0; i < num; i++)
        {
            array[i] = FromByteArrayImpl(new ArraySegment<byte>(bytes, i * 8, 8).Array!);
        }
        return array;
    }

    private static DateTime FromByteArrayImpl(byte[] bytes)
    {
        if (bytes.Length != 8)
        {
            throw new ArgumentOutOfRangeException(nameof(bytes), bytes.Length, $"Parsing a DateTime requires exactly 8 bytes of input data, input data is {bytes.Length} bytes long.");
        }

        var year = ByteToYear(bytes[0]);
        var month = AssertRangeInclusive(DecodeBcd(bytes[1]), 1, 12, "month");
        var day = AssertRangeInclusive(DecodeBcd(bytes[2]), 1, 31, "day of month");
        var hour = AssertRangeInclusive(DecodeBcd(bytes[3]), 0, 23, "hour");
        var minute = AssertRangeInclusive(DecodeBcd(bytes[4]), 0, 59, "minute");
        var second = AssertRangeInclusive(DecodeBcd(bytes[5]), 0, 59, "second");
        var num = AssertRangeInclusive(DecodeBcd(bytes[6]), 0, 99, "first two millisecond digits");
        var num2 = AssertRangeInclusive(bytes[7] >> 4, 0, 9, "third millisecond digit");
        return new DateTime(year, month, day, hour, minute, second, num * 10 + num2);

        static int AssertRangeInclusive(int input, byte min, byte max, string field)
        {
            if (input < min)
            {
                throw new ArgumentOutOfRangeException(nameof(input), input, $"Value '{input}' is lower than the minimum '{min}' allowed for {field}.");
            }
            if (input > max)
            {
                throw new ArgumentOutOfRangeException(nameof(input), input, $"Value '{input}' is higher than the maximum '{max}' allowed for {field}.");
            }
            return input;
        }

        static int ByteToYear(byte bcdYear)
        {
            var num4 = DecodeBcd(bcdYear);
            if (num4 < 90)
            {
                return num4 + 2000;
            }
            if (num4 >= 100)
            {
                throw new ArgumentOutOfRangeException(nameof(bcdYear), bcdYear, $"Value '{num4}' is higher than the maximum '99' of S7 date and time representation.");
            }
            return num4 + 1900;
        }

        static int DecodeBcd(byte input)
        {
            return 10 * (input >> 4) + (input & 0xF);
        }
    }

    /// <summary>
    /// Converts a <see cref="DateTime" /> value to a byte array.
    /// </summary>
    /// <param name="dateTime">The DateTime value to convert.</param>
    /// <returns>A byte array containing the S7 date time representation of <paramref name="dateTime" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of
    ///   <paramref name="dateTime" /> is before <see cref="SpecMinimumDateTime" />
    ///   or after <see cref="SpecMaximumDateTime" />.</exception>
    public static byte[] ToByteArray(DateTime dateTime)
    {
        if (dateTime < SpecMinimumDateTime)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), dateTime, $"Date time '{dateTime}' is before the minimum '{SpecMinimumDateTime}' supported in S7 date time representation.");
        }
        if (dateTime > SpecMaximumDateTime)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), dateTime, $"Date time '{dateTime}' is after the maximum '{SpecMaximumDateTime}' supported in S7 date time representation.");
        }

        return
        [
            EncodeBcd(MapYear(dateTime.Year)),
            EncodeBcd(dateTime.Month),
            EncodeBcd(dateTime.Day),
            EncodeBcd(dateTime.Hour),
            EncodeBcd(dateTime.Minute),
            EncodeBcd(dateTime.Second),
            EncodeBcd(dateTime.Millisecond / 10),
            (byte)(dateTime.Millisecond % 10 << 4 | DayOfWeekToInt(dateTime.DayOfWeek))
        ];

        static int DayOfWeekToInt(DayOfWeek dayOfWeek)
        {
            return (int)(dayOfWeek + 1);
        }

        static byte EncodeBcd(int value)
        {
            return (byte)(value / 10 << 4 | value % 10);
        }

        static byte MapYear(int year)
        {
            return (byte)(year < 2000 ? year - 1900 : year - 2000);
        }
    }

    /// <summary>
    /// Converts an array of <see cref="DateTime" /> values to a byte array.
    /// </summary>
    /// <param name="dateTimes">The DateTime values to convert.</param>
    /// <returns>A byte array containing the S7 date time representations of <paramref name="dateTimes" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any value of
    ///   <paramref name="dateTimes" /> is before <see cref="SpecMinimumDateTime" />
    ///   or after <see cref="SpecMaximumDateTime" />.</exception>
    public static byte[] ToByteArray(DateTime[] dateTimes)
    {
        var list = new List<byte>(dateTimes.Length * 8);
        foreach (var dateTime in dateTimes)
        {
            list.AddRange(ToByteArray(dateTime));
        }
        return [.. list];
    }
}
