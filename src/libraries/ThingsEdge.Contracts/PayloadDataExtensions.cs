using Bit = System.Boolean;
using Word = System.UInt16;
using DWord = System.UInt32;
using Int = System.Int16;
using DInt = System.Int32;
using Real = System.Single;
using LReal = System.Double;

namespace ThingsEdge.Contracts;

public static class PayloadDataExtensions
{
    /// <summary>
    /// 获取 <see cref="DataType.String"/> 或 <see cref="DataType.S7String"/> 或 <see cref="DataType.S7WString"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <remarks>注：对于非字符串数据，会转换为字符串；对于数组数据，进行 JSON 序列化。</remarks>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static string GetString(this PayloadData payload)
    {
        var v = payload.GetValue();
        return v.Match(
            t0 => t0,
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
    /// 获取 <see cref="DataType.Bit"/> 类型的值。
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
    /// 获取 <see cref="DataType.Byte"/> 类型的值。
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
    /// 获取 <see cref="DataType.Word"/> 类型的值。
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
    /// 获取 <see cref="DataType.DWord"/> 类型的值。
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
    /// 获取 <see cref="DataType.Int"/> 类型的值。
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
    /// 获取 <see cref="DataType.DInt"/> 类型的值。
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
    /// 获取 <see cref="DataType.Real"/> 类型的值。
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
    /// 获取 <see cref="DataType.LReal"/> 类型的值。
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
    /// 获取 <see cref="DataType.Bit"/> 数组类型的值。
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
    /// 获取 <see cref="DataType.Byte"/> 数组类型的值。
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
    /// 获取 <see cref="DataType.Word"/> 数组类型的值。
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
    /// 获取 <see cref="DataType.DWord"/> 数组类型的值。
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
    /// 获取 <see cref="DataType.Int"/> 数组类型的值。
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
    /// 获取 <see cref="DataType.DInt"/> 数组类型的值。
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
    /// 获取 <see cref="DataType.Real"/> 数组类型的值。
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
    /// 获取 <see cref="DataType.LReal"/> 数组类型的值。
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
        if (payload.Length == 0) // payload.Value.GetType().IsArray
        {
            switch (payload.DataType)
            {
                case DataType.Bit: return (Bit)payload.Value;
                case DataType.Byte: return ((byte[])payload.Value)[0]; // byte 都为数组
                case DataType.Word: return (Word)payload.Value;
                case DataType.DWord: return (DWord)payload.Value;
                case DataType.Int: return (Int)payload.Value;
                case DataType.DInt: return (DInt)payload.Value;
                case DataType.Real: return (Real)payload.Value;
                case DataType.LReal: return (LReal)payload.Value;
                case DataType.String or DataType.S7String or DataType.S7WString: return (string)payload.Value;
                default: throw new FormatException();
            }
        }

        // 数组
        switch (payload.DataType)
        {
            case DataType.Bit: return (Bit[])payload.Value;
            case DataType.Byte: return (byte[])payload.Value;
            case DataType.Word: return (Word[])payload.Value;
            case DataType.DWord: return (DWord[])payload.Value;
            case DataType.Int: return (Int[])payload.Value;
            case DataType.DInt: return (DInt[])payload.Value;
            case DataType.Real: return (Real[])payload.Value;
            case DataType.LReal: return (LReal[])payload.Value;
            case DataType.String or DataType.S7String or DataType.S7WString: return Arr2Str((string[])payload.Value);
            default: throw new FormatException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Arr2Str<T>(T[] items)
    {
        return JsonSerializer.Serialize(items);
    }
}
