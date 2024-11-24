using ThingsEdge.Exchange.Contracts.Utils;
using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Contracts;

/// <summary>
/// <see cref="PayloadData"/> 直接提取数据的扩展对象。
/// </summary>
public static partial class PayloadDataExtensions
{
    /// <summary>
    /// 尝试获取值并将值转换为 <see cref="bool"/> 类型。
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks>
    /// 仅将原始类型 Bit 转换为 Boolean 类型。
    /// </remarks>
    public static bool TryGetAsBoolean(this PayloadData payload, [NotNullWhen(true)] out bool? value)
    {
        if (payload.DataType == TagDataType.Bit)
        {
            value = payload.GetBit();
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// 尝试获取值并将值转换为 <see cref="int"/> 类型。
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks>
    /// 会将原始类型 Bit、Byte、Word、Int 和 DInt 转换为 Int32 类型。
    /// </remarks>
    public static bool TryGetAsInt32(this PayloadData payload, [NotNullWhen(true)] out int? value)
    {
        switch (payload.DataType)
        {
            case TagDataType.Bit:
                value = Convert.ToInt32(payload.GetBit());
                return true;
            case TagDataType.Byte:
                value = payload.GetByte();
                return true;
            case TagDataType.Word:
                value = payload.GetWord();
                return true;
            case TagDataType.Int:
                value = payload.GetInt();
                return true;
            case TagDataType.DInt:
                value = payload.GetDInt();
                return true;
            default:
                break;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// 尝试获取值并将值转换为 <see cref="double"/> 类型。
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks>
    /// 会将原始类型 Bit、Byte、Word、DWord、Int、DInt、Real 和 LReal 转换为 double 类型。
    /// </remarks>
    public static bool TryGetAsDouble(this PayloadData payload, [NotNullWhen(true)] out double? value)
    {
        switch (payload.DataType)
        {
            case TagDataType.Bit:
                value = Convert.ToDouble(payload.GetBit());
                return true;
            case TagDataType.Byte:
                value = payload.GetByte();
                return true;
            case TagDataType.Word:
                value = payload.GetWord();
                return true;
            case TagDataType.DWord:
                value = payload.GetDWord();
                return true;
            case TagDataType.Int:
                value = payload.GetInt();
                return true;
            case TagDataType.DInt:
                value = payload.GetDInt();
                return true;
            case TagDataType.Real:
                value = payload.GetReal();
                return true;
            case TagDataType.LReal:
                value = payload.GetLReal();
                return true;
            default:
                break;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// 尝试获取值并将值转换为 <see cref="bool"/> 数组类型。
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks>
    /// 仅将原始类型 Bit 转换为 Boolean 类型。
    /// </remarks>
    public static bool TryGetAsBooleanArray(this PayloadData payload, [NotNullWhen(true)] out bool[]? value)
    {
        if (payload.IsArray() && payload.DataType == TagDataType.Bit)
        {
            value = ConvertUtil.ToBooleanArray(payload.Value);
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// 尝试获取值并将值转换为 <see cref="int"/> 数组类型。
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks>
    /// 会将原始类型 Bit、Byte、Word、Int 和 DInt 转换为 Int32 类型。
    /// </remarks>
    public static bool TryGetAsInt32Array(this PayloadData payload, [NotNullWhen(true)] out int[]? value)
    {
        if (payload.IsArray())
        {
            if (payload.DataType is TagDataType.Bit
                or TagDataType.Byte or TagDataType.Word
                or TagDataType.Int or TagDataType.DInt)
            {
                value = ConvertUtil.ToArray(payload.Value, Convert.ToInt32);
                return true;
            }
        }

        value = null;
        return false;
    }

    /// <summary>
    /// 尝试获取值并将值转换为 <see cref="double"/> 数组类型。
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks>
    /// 会将原始类型 Bit、Byte、Word、DWord、Int、DInt、Real 和 LReal 转换为 double 类型。
    /// </remarks>
    public static bool TryGetAsDoubleArray(this PayloadData payload, [NotNullWhen(true)] out double[]? value)
    {
        if (payload.IsArray())
        {
            if (payload.DataType is TagDataType.Bit or TagDataType.Byte
                or TagDataType.Word or TagDataType.DWord
                or TagDataType.Int or TagDataType.DInt
                or TagDataType.Real or TagDataType.LReal)
            {
                value = ConvertUtil.ToArray(payload.Value, Convert.ToDouble);
                return true;
            }
        }

        value = null;
        return false;
    }
}
