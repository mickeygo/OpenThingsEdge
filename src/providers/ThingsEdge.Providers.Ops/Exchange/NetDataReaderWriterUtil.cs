using Ops.Communication;
using Ops.Communication.Core;
using Ops.Communication.Profinet.Siemens;

namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// 驱动数据读取。
/// </summary>
internal static class NetDataReaderWriterUtil
{
    /// <summary>
    /// 读取数据。
    /// </summary>
    /// <param name="driver">读取数据。</param>
    /// <param name="tag">Tag标记</param>
    /// <returns></returns>
    public static async Task<(bool ok, PayloadData data, string err)> ReadSingleAsync(IReadWriteNet driver, Tag tag)
    {
        bool ok = false;
        string errMessage = string.Empty;
        var payloadData = new PayloadData
        {
            TagId = tag.TagId,
            TagName = tag.Name,
            Address = tag.Address,
            Length = tag.Length,
            DataType = tag.DataType,
            Keynote = tag.Keynote,
        };
        switch (tag.DataType)
        {
            case DataType.Bit:
                if (tag.Length > 0)
                {
                    var resultBit2 = await driver.ReadBoolAsync(tag.Address, (ushort)tag.Length);
                    SetValue(resultBit2, payloadData);
                }
                else
                {
                    var resultBit1 = await driver.ReadBoolAsync(tag.Address);
                    SetValue(resultBit1, payloadData);
                }
                break;
            case DataType.Byte:
                if (tag.Length > 0)
                {
                    var resultBit2 = await driver.ReadAsync(tag.Address, (ushort)tag.Length);
                    SetValue(resultBit2, payloadData);
                }
                else
                {
                    var resultBit1 = await driver.ReadAsync(tag.Address, 1);
                    SetValue(resultBit1, payloadData);
                }
                break;
            case DataType.Word:
                if (tag.Length > 0)
                {
                    var resultWord2 = await driver.ReadUInt16Async(tag.Address, (ushort)tag.Length);
                    SetValue(resultWord2, payloadData);
                }
                else
                {
                    var resultWord1 = await driver.ReadUInt16Async(tag.Address);
                    SetValue(resultWord1, payloadData);
                }
                break;
            case DataType.DWord:
                if (tag.Length > 0)
                {
                    var resultUInt2 = await driver.ReadUInt32Async(tag.Address, (ushort)tag.Length);
                    SetValue(resultUInt2, payloadData);
                }
                else
                {
                    var resultUInt1 = await driver.ReadUInt32Async(tag.Address);
                    SetValue(resultUInt1, payloadData);
                }
                break;
            case DataType.Int:
                if (tag.Length > 0)
                {
                    var resultInt2 = await driver.ReadInt16Async(tag.Address, (ushort)tag.Length);
                    SetValue(resultInt2, payloadData);
                }
                else
                {
                    var resultInt1 = await driver.ReadInt16Async(tag.Address);
                    SetValue(resultInt1, payloadData);
                }
                break;
            case DataType.DInt:
                if (tag.Length > 0)
                {
                    var resultDInt2 = await driver.ReadInt32Async(tag.Address, (ushort)tag.Length);
                    SetValue(resultDInt2, payloadData);
                }
                else
                {
                    var resultDInt1 = await driver.ReadInt32Async(tag.Address);
                    SetValue(resultDInt1, payloadData);
                }
                break;
            case DataType.Real:
                if (tag.Length > 0)
                {
                    var resultReal2 = await driver.ReadFloatAsync(tag.Address, (ushort)tag.Length);
                    SetValue(resultReal2, payloadData);
                }
                else
                {
                    var resultReal1 = await driver.ReadFloatAsync(tag.Address);
                    SetValue(resultReal1, payloadData);
                }
                break;
            case DataType.LReal:
                if (tag.Length > 0)
                {
                    var resultLReal2 = await driver.ReadDoubleAsync(tag.Address, (ushort)tag.Length);
                    SetValue(resultLReal2, payloadData);
                }
                else
                {
                    var resultLReal1 = await driver.ReadDoubleAsync(tag.Address);
                    SetValue(resultLReal1, payloadData);
                }
                break;
            case DataType.String or DataType.S7String:
                if (driver is SiemensS7Net driver1)
                {
                    var resultString1 = await driver1.ReadStringAsync(tag.Address); // S7 自动计算长度
                    SetValue(resultString1, payloadData);
                }
                else
                {
                    var resultString2 = await driver.ReadStringAsync(tag.Address, (ushort)tag.Length);
                    SetValue(resultString2, payloadData);
                }
                break;
            case DataType.S7WString:
                if (driver is SiemensS7Net driver2)
                {
                    var resultWString2 = await driver2.ReadWStringAsync(tag.Address);
                    SetValue(resultWString2, payloadData);
                }
                break;
            default:
                break;
        }

        return (ok, payloadData, errMessage);

        void SetValue<T>(OperateResult<T> result, PayloadData data)
        {
            if (result.IsSuccess)
            {
                data.Value = result.Content!;
                ok = true;
            }
            else
            {
                ok = false;
                errMessage = result.Message;
            }
        }
    }

    /// <summary>
    /// 写入数据。
    /// </summary>
    /// <param name="driver">驱动器。</param>
    /// <param name="data">要写入的数据。</param>
    /// <returns></returns>
    public static async Task<(bool ok, string err)> WriteSingleAsync(IReadWriteNet driver, PayloadData data)
    {
        OperateResult operateResult;
        switch (data.DataType)
        {
            case DataType.Bit:
                if (data.Length > 0)
                {
                    operateResult = await driver.WriteAsync(data.Address, (bool[])data.Value);
                }
                else
                {
                    operateResult = await driver.WriteAsync(data.Address, (bool)data.Value);
                }
                break;
            case DataType.Byte:
                byte[] bytes = data.Length > 0 ? (byte[])data.Value : new byte[] { (byte)data.Value };
                operateResult = await driver.WriteAsync(data.Address, bytes);
                break;
            case DataType.Word:
                if (data.Length > 0)
                {
                    operateResult = await driver.WriteAsync(data.Address, (ushort[])data.Value);
                }
                else
                {
                    operateResult = await driver.WriteAsync(data.Address, (ushort)data.Value);
                }
                break;
            case DataType.DWord:
                if (data.Length > 0)
                {
                    operateResult = await driver.WriteAsync(data.Address, (uint[])data.Value);
                }
                else
                {
                    operateResult = await driver.WriteAsync(data.Address, (uint)data.Value);
                }
                break;
            case DataType.Int:
                if (data.Length > 0)
                {
                    operateResult = await driver.WriteAsync(data.Address, (short[])data.Value);
                }
                else
                {
                    operateResult = await driver.WriteAsync(data.Address, (short)data.Value);
                }
                break;
            case DataType.DInt:
                if (data.Length > 0)
                {
                    operateResult = await driver.WriteAsync(data.Address, (int[])data.Value);
                }
                else
                {
                    operateResult = await driver.WriteAsync(data.Address, (int)data.Value);
                }
                break;
            case DataType.Real:
                if (data.Length > 0)
                {
                    operateResult = await driver.WriteAsync(data.Address, (float[])data.Value);
                }
                else
                {
                    operateResult = await driver.WriteAsync(data.Address, (float)data.Value);
                }
                break;
            case DataType.LReal:
                if (data.Length > 0)
                {
                    operateResult = await driver.WriteAsync(data.Address, (double[])data.Value);
                }
                else
                {
                    operateResult = await driver.WriteAsync(data.Address, (double)data.Value);
                }
                break;
            case DataType.String or DataType.S7String:
                operateResult = await driver.WriteAsync(data.Address, data.Value.ToString());
                break;
            case DataType.S7WString:
            default:
                if (driver is SiemensS7Net driver2)
                {
                    operateResult = await driver2.WriteWStringAsync(data.Address, data.Value.ToString());
                }
                else
                {
                    operateResult = await driver.WriteAsync(data.Address, data.Value.ToString());
                }
                break;
        }

        return (operateResult.IsSuccess, operateResult.Message);
    }

    /// <summary>
    /// 写入数据。
    /// </summary>
    /// <param name="driver">驱动器。</param>
    /// <param name="tag">要写入的标记。</param>
    /// <param name="data">要写入的数据。</param>
    /// <returns></returns>
    /// <remarks>要写入的数据必须与标记的数据类型匹配，或是可转换为标记设定的类型。</remarks>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="InvalidCastException"></exception>
    /// <exception cref="OverflowException"></exception>
    public static async Task<(bool ok, string err)> WriteSingleAsync(IReadWriteNet driver, Tag tag, object data)
    {
        OperateResult operateResult;
        switch (tag.DataType)
        {
            case DataType.Bit:
                if (tag.Length > 0)
                {
                    throw new InvalidCastException();
                }
                else
                {
                    operateResult = await driver.WriteAsync(tag.Address, Convert.ToBoolean(data));
                }
                break;
            case DataType.Byte:
                byte[] bytes = tag.Length > 0 ? (byte[])data : new byte[] { Convert.ToByte(data) };
                operateResult = await driver.WriteAsync(tag.Address, bytes);
                break;
            case DataType.Word:
                if (tag.Length > 0)
                {
                    throw new InvalidCastException();
                }
                else
                {
                    operateResult = await driver.WriteAsync(tag.Address, Convert.ToUInt16(data));
                }
                break;
            case DataType.DWord:
                if (tag.Length > 0)
                {
                    throw new InvalidCastException();
                }
                else
                {
                    operateResult = await driver.WriteAsync(tag.Address, Convert.ToUInt32(data));
                }
                break;
            case DataType.Int:
                if (tag.Length > 0)
                {
                    throw new InvalidCastException();
                }
                else
                {
                    operateResult = await driver.WriteAsync(tag.Address, Convert.ToInt16(data));
                }
                break;
            case DataType.DInt:
                if (tag.Length > 0)
                {
                    throw new InvalidCastException();
                }
                else
                {
                    operateResult = await driver.WriteAsync(tag.Address, Convert.ToInt32(data));
                }
                break;
            case DataType.Real:
                if (tag.Length > 0)
                {
                    throw new InvalidCastException();
                }
                else
                {
                    operateResult = await driver.WriteAsync(tag.Address, Convert.ToSingle(data));
                }
                break;
            case DataType.LReal:
                if (tag.Length > 0)
                {
                    throw new InvalidCastException();
                }
                else
                {
                    operateResult = await driver.WriteAsync(tag.Address, Convert.ToDouble(data));
                }
                break;
            case DataType.String or DataType.S7String:
                operateResult = await driver.WriteAsync(tag.Address, data.ToString());
                break;
            case DataType.S7WString:
            default:
                if (driver is SiemensS7Net driver2)
                {
                    operateResult = await driver2.WriteWStringAsync(tag.Address, data.ToString());
                }
                else
                {
                    operateResult = await driver.WriteAsync(tag.Address, data.ToString());
                }
                break;
        }

        return (operateResult.IsSuccess, operateResult.Message);
    }
}
