using Bit = System.Boolean;
using Word = System.UInt16;
using DWord = System.UInt32;
using Int = System.Int16;
using DInt = System.Int32;
using Real = System.Single;
using LReal = System.Double;

namespace ThingsEdge.Contracts;

/// <summary>
/// <see cref="PayloadData"/> 直接提取数据的扩展对象。
/// </summary>
public static class PayloadDataExtensions
{
    /// <summary>
    /// 提取对象文本值，若值不为 <see cref="string"/> 类型时，会字符串转换。
    /// </summary>
    /// <remarks>注：对于非字符串数据，会转换为字符串；对于数组数据，进行 JSON 序列化，返回的是 JSON 数组格式文本。</remarks>
    /// <param name="payload"></param>
    /// <param name="isTrimString">是否移除字符串首尾 '\0' 和 ' ' 字符，默认为 true。</param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static string GetString(this PayloadData payload, bool isTrimString = true)
    {
        var v = payload.GetValue();
        return v.Match(
            t0 => isTrimString ? t0.Trim('\0', ' ') : t0,
            t1 => t1.ToString(),
            t2 => t2.ToString(),
            t3 => t3.ToString(),
            t4 => t4.ToString(),
            t5 => t5.ToString(),
            t6 => t6.ToString(),
            t7 => t7.ToString(),
            t8 => t8.ToString(),
            t9 => Arr2Str(t9),
            t10 => Arr2Str(t10),
            t11 => Arr2Str(t11),
            t12 => Arr2Str(t12),
            t13 => Arr2Str(t13),
            t14 => Arr2Str(t14),
            t15 => Arr2Str(t15),
            t16 => Arr2Str(t16)
            );
    }

    /// <summary>
    /// 提取对象值，并将值转换为字符串数组，若是单一值，会组合成只有一个元素的数组。
    /// </summary>
    /// <remarks>注：对于非字符串数据，会转换为字符串；对于数组数据，进行 JSON 序列化，返回的是 JSON 数组格式文本。</remarks>
    /// <param name="payload"></param>
    /// <param name="isTrimString">是否移除字符串首尾 '\0' 和 ' ' 字符，默认为 true。</param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static string[] GetStringArray(this PayloadData payload, bool isTrimString = true)
    {
        var v = payload.GetValue();
        return v.Match(
            t0 => [isTrimString ? t0.Trim('\0', ' ') : t0],
            t1 => [t1.ToString()],
            t2 => [t2.ToString()],
            t3 => [t3.ToString()],
            t4 => [t4.ToString()],
            t5 => [t5.ToString()],
            t6 => [t6.ToString()],
            t7 => [t7.ToString()],
            t8 => [t8.ToString()],
            t9 => t9.Select(s => s.ToString()).ToArray(),
            t10 => t10.Select(s => s.ToString()).ToArray(),
            t11 => t11.Select(s => s.ToString()).ToArray(),
            t12 => t12.Select(s => s.ToString()).ToArray(),
            t13 => t13.Select(s => s.ToString()).ToArray(),
            t14 => t14.Select(s => s.ToString()).ToArray(),
            t15 => t15.Select(s => s.ToString()).ToArray(),
            t16 => t16.Select(s => s.ToString()).ToArray()
            );
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.Bit"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static Bit GetBit(this PayloadData payload)
    {
        var v = payload.GetValue();
        return v.AsT1;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.Byte"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static byte GetByte(this PayloadData payload)
    {
        var v = payload.GetValue();
        return v.AsT2;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.Word"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static Word GetWord(this PayloadData payload)
    {
        var v = payload.GetValue();
        return v.AsT3;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.DWord"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static DWord GetDWord(this PayloadData payload)
    {
        var v = payload.GetValue();
        return v.AsT4;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.Int"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static Int GetInt(this PayloadData payload)
    {
        var v = payload.GetValue();
        return v.AsT5;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.DInt"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static DInt GetDInt(this PayloadData payload)
    {
        var v = payload.GetValue();
        return v.AsT6;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.Real"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static Real GetReal(this PayloadData payload)
    {
        var v = payload.GetValue();
        return v.AsT7;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.LReal"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static LReal GetLReal(this PayloadData payload)
    {
        var v = payload.GetValue();
        return v.AsT8;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.Bit"/> 数组类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static Bit[] GetBitArray(this PayloadData payload)
    {
        var v = payload.GetValue();
        return v.AsT9;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.Byte"/> 数组类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static byte[] GetByteArray(this PayloadData payload)
    {
        var v = payload.GetValue();
        return v.AsT10;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.Word"/> 数组类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static Word[] GetWordArray(this PayloadData payload)
    {
        var v = payload.GetValue();
        return v.AsT11;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.DWord"/> 数组类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static DWord[] GetDWordArray(this PayloadData payload)
    {
        var v = payload.GetValue();
        return v.AsT12;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.Int"/> 数组类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static Int[] GetIntArray(this PayloadData payload)
    {
        var v = payload.GetValue();
        return v.AsT13;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.DInt"/> 数组类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static DInt[] GetDIntArray(this PayloadData payload)
    {
        var v = payload.GetValue();
        return v.AsT14;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.Real"/> 数组类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static Real[] GetRealArray(this PayloadData payload)
    {
        var v = payload.GetValue();
        return v.AsT15;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.LReal"/> 数组类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static LReal[] GetDRealArray(this PayloadData payload)
    {
        var v = payload.GetValue();
        return v.AsT16;
    }

    private static OneOf<string, Bit, byte, Word, DWord, Int, DInt, Real, LReal, Bit[], byte[], Word[], DWord[], Int[], DInt[], Real[], LReal[]> GetValue(this PayloadData payload)
    {
        // 单值
        if (!payload.IsArray()) // payload.Value.GetType().IsArray
        {
            return payload.DataType switch
            {
                TagDataType.Bit => (Bit)payload.Value,
                TagDataType.Byte => ((byte[])payload.Value)[0],// byte 都为数组
                TagDataType.Word => (Word)payload.Value,
                TagDataType.DWord => (DWord)payload.Value,
                TagDataType.Int => (Int)payload.Value,
                TagDataType.DInt => (DInt)payload.Value,
                TagDataType.Real => (Real)payload.Value,
                TagDataType.LReal => (LReal)payload.Value,
                TagDataType.String or TagDataType.S7String or TagDataType.S7WString => (string)payload.Value,
                _ => throw new FormatException(),
            };
        }

        // 数组
        return payload.DataType switch
        {
            TagDataType.Bit => (Bit[])payload.Value,
            TagDataType.Byte => (byte[])payload.Value,
            TagDataType.Word => (Word[])payload.Value,
            TagDataType.DWord => (DWord[])payload.Value,
            TagDataType.Int => (Int[])payload.Value,
            TagDataType.DInt => (DInt[])payload.Value,
            TagDataType.Real => (Real[])payload.Value,
            TagDataType.LReal => (LReal[])payload.Value,
            TagDataType.String or TagDataType.S7String or TagDataType.S7WString => (string)payload.Value,
            _ => throw new FormatException(),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Arr2Str<T>(T[] items)
    {
        return JsonSerializer.Serialize(items);
    }
}
