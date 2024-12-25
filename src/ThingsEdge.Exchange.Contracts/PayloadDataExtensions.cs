using Bit = System.Boolean;
using Word = System.UInt16;
using DWord = System.UInt32;
using Int = System.Int16;
using DInt = System.Int32;
using Real = System.Single;
using LReal = System.Double;

using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Contracts;

/// <summary>
/// <see cref="PayloadData"/> 直接提取数据的扩展对象。
/// </summary>
public static partial class PayloadDataExtensions
{
    /// <summary>
    /// 提取对象文本值，若值不为 <see cref="string"/> 类型时，会转换为字符串。
    /// </summary>
    /// <remarks>注：对于非字符串数据，会转换为字符串；对于数组数据，进行 JSON 序列化，返回的是 JSON 数组格式文本。</remarks>
    /// <param name="payload"></param>
    /// <param name="isTrimString">是否移除字符串首尾 '\0' 和 ' ' 字符，默认为 true。</param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static string GetString(this PayloadData payload, bool isTrimString = true)
    {
        if (payload.DataType is TagDataType.String or TagDataType.S7String or TagDataType.S7WString)
        {
            var s = (string)payload.Value;
            return isTrimString ? s.Trim('\0', ' ') : s;
        }

        var array = payload.IsArray();
        return payload.DataType switch
        {
            TagDataType.Bit => !array ? payload.GetBit().ToString() : Arr2Str(payload.GetBitArray()),
            TagDataType.Byte => !array ? payload.GetByte().ToString() : Arr2Str(payload.GetByteArray()),
            TagDataType.Word => !array ? payload.GetWord().ToString() : Arr2Str(payload.GetWordArray()),
            TagDataType.DWord => !array ? payload.GetDWord().ToString() : Arr2Str(payload.GetDWordArray()),
            TagDataType.Int => !array ? payload.GetInt().ToString() : Arr2Str(payload.GetIntArray()),
            TagDataType.DInt => !array ? payload.GetDInt().ToString() : Arr2Str(payload.GetDIntArray()),
            TagDataType.Real => !array ? payload.GetReal().ToString() : Arr2Str(payload.GetRealArray()),
            TagDataType.LReal => !array ? payload.GetLReal().ToString() : Arr2Str(payload.GetLRealArray()),
            _ => throw new FormatException(),
        };
    }

    /// <summary>
    /// 提取对象值，并将值转换为字符串数组，若是单一值，会组合成只有一个元素的数组。
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="isTrimString">是否移除字符串首尾 '\0' 和 ' ' 字符，默认为 true。</param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static string[] GetStringArray(this PayloadData payload, bool isTrimString = true)
    {
        if (payload.DataType is TagDataType.String or TagDataType.S7String or TagDataType.S7WString)
        {
            var s = (string)payload.Value;
            return [isTrimString ? s.Trim('\0', ' ') : s];
        }

        var array = payload.IsArray();
        return payload.DataType switch
        {
            TagDataType.Bit => !array ? [payload.GetBit().ToString()] : payload.GetBitArray().Select(s => s.ToString()).ToArray(),
            TagDataType.Byte => !array ? [payload.GetByte().ToString()] : payload.GetByteArray().Select(s => s.ToString()).ToArray(),
            TagDataType.Word => !array ? [payload.GetWord().ToString()] : payload.GetWordArray().Select(s => s.ToString()).ToArray(),
            TagDataType.DWord => !array ? [payload.GetDWord().ToString()] : payload.GetDWordArray().Select(s => s.ToString()).ToArray(),
            TagDataType.Int => !array ? [payload.GetInt().ToString()] : payload.GetIntArray().Select(s => s.ToString()).ToArray(),
            TagDataType.DInt => !array ? [payload.GetDInt().ToString()] : payload.GetDIntArray().Select(s => s.ToString()).ToArray(),
            TagDataType.Real => !array ? [payload.GetReal().ToString()] : payload.GetRealArray().Select(s => s.ToString()).ToArray(),
            TagDataType.LReal => !array ? [payload.GetLReal().ToString()] : payload.GetLRealArray().Select(s => s.ToString()).ToArray(),
            _ => throw new FormatException(),
        };
    }

    /// <summary>
    /// 获取原始的字符串数据。
    /// 数据类型必须为 <see cref="TagDataType.String"/>、 <see cref="TagDataType.S7String"/> 或  <see cref="TagDataType.S7WString"/> 类型的值。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static string GetRawString(this PayloadData payload)
    {
        if (payload.DataType is not (TagDataType.String or TagDataType.S7String or TagDataType.S7WString))
        {
            throw new FormatException();
        }

        return (string)payload.Value;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.Bit"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static Bit GetBit(this PayloadData payload)
    {
        if (payload.DataType != TagDataType.Bit)
        {
            throw new FormatException();
        }

        return (Bit)payload.Value;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.Byte"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static byte GetByte(this PayloadData payload)
    {
        if (payload.DataType != TagDataType.Byte)
        {
            throw new FormatException();
        }

        return ((byte[])payload.Value)[0];
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.Word"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static Word GetWord(this PayloadData payload)
    {
        if (payload.DataType != TagDataType.Word)
        {
            throw new FormatException();
        }

        return (Word)payload.Value;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.DWord"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static DWord GetDWord(this PayloadData payload)
    {
        if (payload.DataType != TagDataType.DWord)
        {
            throw new FormatException();
        }

        return (DWord)payload.Value;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.Int"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static Int GetInt(this PayloadData payload)
    {
        if (payload.DataType != TagDataType.Int)
        {
            throw new FormatException();
        }

        return (Int)payload.Value;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.DInt"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static DInt GetDInt(this PayloadData payload)
    {
        if (payload.DataType != TagDataType.DInt)
        {
            throw new FormatException();
        }

        return (DInt)payload.Value;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.Real"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static Real GetReal(this PayloadData payload)
    {
        if (payload.DataType != TagDataType.Real)
        {
            throw new FormatException();
        }

        return (Real)payload.Value;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.LReal"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static LReal GetLReal(this PayloadData payload)
    {
        if (payload.DataType != TagDataType.LReal)
        {
            throw new FormatException();
        }

        return (LReal)payload.Value;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.Bit"/> 数组类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static Bit[] GetBitArray(this PayloadData payload)
    {
        if (payload.DataType != TagDataType.Bit)
        {
            throw new FormatException();
        }

        return (Bit[])payload.Value;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.Byte"/> 数组类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static byte[] GetByteArray(this PayloadData payload)
    {
        if (payload.DataType != TagDataType.Byte)
        {
            throw new FormatException();
        }

        return (byte[])payload.Value;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.Word"/> 数组类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static Word[] GetWordArray(this PayloadData payload)
    {
        if (payload.DataType != TagDataType.Word)
        {
            throw new FormatException();
        }

        return (Word[])payload.Value;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.DWord"/> 数组类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static DWord[] GetDWordArray(this PayloadData payload)
    {
        if (payload.DataType != TagDataType.DWord)
        {
            throw new FormatException();
        }

        return (DWord[])payload.Value;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.Int"/> 数组类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static Int[] GetIntArray(this PayloadData payload)
    {
        if (payload.DataType != TagDataType.Int)
        {
            throw new FormatException();
        }

        return (Int[])payload.Value;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.DInt"/> 数组类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static DInt[] GetDIntArray(this PayloadData payload)
    {
        if (payload.DataType != TagDataType.DInt)
        {
            throw new FormatException();
        }

        return (DInt[])payload.Value;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.Real"/> 数组类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static Real[] GetRealArray(this PayloadData payload)
    {
        if (payload.DataType != TagDataType.Real)
        {
            throw new FormatException();
        }

        return (Real[])payload.Value;
    }

    /// <summary>
    /// 获取 <see cref="TagDataType.LReal"/> 数组类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static LReal[] GetLRealArray(this PayloadData payload)
    {
        if (payload.DataType != TagDataType.LReal)
        {
            throw new FormatException();
        }

        return (LReal[])payload.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Arr2Str<T>(T[] items)
    {
        return JsonSerializer.Serialize(items);
    }
}
