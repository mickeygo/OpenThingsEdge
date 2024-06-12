using Ops.Communication;
using Ops.Communication.Core;
using Ops.Communication.Profinet.Siemens;

namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// 驱动数据读取帮助类。
/// </summary>
internal static class DriverReadWriteUtil
{
    /// <summary>
    /// 读取数据。
    /// </summary>
    /// <param name="driver">读取数据。</param>
    /// <param name="tag">Tag标记</param>
    /// <returns></returns>
    public static async Task<DriverReadResult> ReadAsync(IReadWriteNet driver, Tag tag)
    {
        // 非数组
        if (!tag.IsArray())
        {
            return tag.DataType switch
            {
                TagDataType.Bit => MakeResult(await driver.ReadBoolAsync(tag.Address).ConfigureAwait(false)),
                TagDataType.Byte => MakeResult(await driver.ReadAsync(tag.Address, 1).ConfigureAwait(false)),
                TagDataType.Word => MakeResult(await driver.ReadUInt16Async(tag.Address).ConfigureAwait(false)),
                TagDataType.DWord => MakeResult(await driver.ReadUInt32Async(tag.Address).ConfigureAwait(false)),
                TagDataType.Int => MakeResult(await driver.ReadInt16Async(tag.Address).ConfigureAwait(false)),
                TagDataType.DInt => MakeResult(await driver.ReadInt32Async(tag.Address).ConfigureAwait(false)),
                TagDataType.Real => MakeResult(await driver.ReadFloatAsync(tag.Address).ConfigureAwait(false)),
                TagDataType.LReal => MakeResult(await driver.ReadDoubleAsync(tag.Address).ConfigureAwait(false)),
                TagDataType.String or TagDataType.S7String => MakeResult(await ReadStringAsync(driver, tag.Address, (ushort)tag.Length).ConfigureAwait(false)),
                TagDataType.S7WString => MakeResult(await ReadWStringAsync(driver, tag.Address).ConfigureAwait(false)),
                _ => throw new NotImplementedException(),
            };
        }

        // 数组
        return tag.DataType switch
        {
            TagDataType.Bit => MakeResult(await driver.ReadBoolAsync(tag.Address, (ushort)tag.Length).ConfigureAwait(false)),
            TagDataType.Byte => MakeResult(await driver.ReadAsync(tag.Address, (ushort)tag.Length).ConfigureAwait(false)),
            TagDataType.Word => MakeResult(await driver.ReadUInt16Async(tag.Address, (ushort)tag.Length).ConfigureAwait(false)),
            TagDataType.DWord => MakeResult(await driver.ReadUInt32Async(tag.Address, (ushort)tag.Length).ConfigureAwait(false)),
            TagDataType.Int => MakeResult(await driver.ReadInt16Async(tag.Address, (ushort)tag.Length).ConfigureAwait(false)),
            TagDataType.DInt => MakeResult(await driver.ReadInt32Async(tag.Address, (ushort)tag.Length).ConfigureAwait(false)),
            TagDataType.Real => MakeResult(await driver.ReadFloatAsync(tag.Address, (ushort)tag.Length).ConfigureAwait(false)),
            TagDataType.LReal => MakeResult(await driver.ReadDoubleAsync(tag.Address, (ushort)tag.Length).ConfigureAwait(false)),
            _ => throw new NotImplementedException(),
        };

        DriverReadResult MakeResult<T>(OperateResult<T> result)
        {
            DriverReadResult result2 = new();

            if (result.IsSuccess)
            {
                result2.Data = PayloadData.FromTag(tag);
                result2.Data.Value = result.Content;
            }
            else
            {
                result2.Code = 2;
                result2.ErrorCode = result.ErrorCode;
                result2.ErrorMessage = result.Message;
            }

            return result2;
        }
    }

    private static async Task<OperateResult<string>> ReadStringAsync(IReadWriteNet driver, string address, ushort length)
    {
        if (driver is SiemensS7Net s7Driver)
        {
            return await s7Driver.ReadStringAsync(address).ConfigureAwait(false); // S7 自动计算长度
        }

        return await driver.ReadStringAsync(address, length).ConfigureAwait(false);
    }

    private static async Task<OperateResult<string>> ReadWStringAsync(IReadWriteNet driver, string address)
    {
        if (driver is SiemensS7Net s7Driver)
        {
            return await s7Driver.ReadWStringAsync(address).ConfigureAwait(false);
        }

        throw new NotImplementedException();
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
    /// <exception cref="NotImplementedException"></exception>
    public static async Task<(bool ok, string err)> WriteAsync(IReadWriteNet driver, Tag tag, object data)
    {
        // 非数组
        if (!tag.IsArray())
        {
            return tag.DataType switch
            {
                TagDataType.Bit => MakeResult(await driver.WriteAsync(tag.Address, (bool)data).ConfigureAwait(false)),
                TagDataType.Byte => MakeResult(await driver.WriteAsync(tag.Address, new byte[] { (byte)data }).ConfigureAwait(false)),
                TagDataType.Word => MakeResult(await driver.WriteAsync(tag.Address, (ushort)data).ConfigureAwait(false)),
                TagDataType.DWord => MakeResult(await driver.WriteAsync(tag.Address, (uint)data).ConfigureAwait(false)),
                TagDataType.Int => MakeResult(await driver.WriteAsync(tag.Address, (short)data).ConfigureAwait(false)),
                TagDataType.DInt => MakeResult(await driver.WriteAsync(tag.Address, (int)data).ConfigureAwait(false)),
                TagDataType.Real => MakeResult(await driver.WriteAsync(tag.Address, (float)data).ConfigureAwait(false)),
                TagDataType.LReal => MakeResult(await driver.WriteAsync(tag.Address, (double)data).ConfigureAwait(false)),
                TagDataType.String or TagDataType.S7String => MakeResult(await WriteStringAsync(driver, tag.Address, (string)data).ConfigureAwait(false)),
                TagDataType.S7WString => MakeResult(await WriteWStringAsync(driver, tag.Address, (string)data).ConfigureAwait(false)),
                _ => throw new NotImplementedException(),
            };
        }

        // 数组
        return tag.DataType switch
        {
            TagDataType.Bit => MakeResult(await driver.WriteAsync(tag.Address, (bool[])data).ConfigureAwait(false)),
            TagDataType.Byte => MakeResult(await driver.WriteAsync(tag.Address, (byte[])data).ConfigureAwait(false)),
            TagDataType.Word => MakeResult(await driver.WriteAsync(tag.Address, (ushort[])data).ConfigureAwait(false)),
            TagDataType.DWord => MakeResult(await driver.WriteAsync(tag.Address, (uint[])data).ConfigureAwait(false)),
            TagDataType.Int => MakeResult(await driver.WriteAsync(tag.Address, (short[])data).ConfigureAwait(false)),
            TagDataType.DInt => MakeResult(await driver.WriteAsync(tag.Address, (int[])data).ConfigureAwait(false)),
            TagDataType.Real => MakeResult(await driver.WriteAsync(tag.Address, (float[])data).ConfigureAwait(false)),
            TagDataType.LReal => MakeResult(await driver.WriteAsync(tag.Address, (double[])data).ConfigureAwait(false)),
            _ => throw new NotImplementedException(),
        };

        static (bool ok, string err) MakeResult(OperateResult result)
        {
            return (result.IsSuccess, result.Message);
        }
    }

    private static async Task<OperateResult> WriteStringAsync(IReadWriteNet driver, string address, string value)
    {
        return await driver.WriteAsync(address, value).ConfigureAwait(false);
    }

    private static async Task<OperateResult> WriteWStringAsync(IReadWriteNet driver, string address, string value)
    {
        if (driver is SiemensS7Net s7Driver)
        {
            return await s7Driver.WriteWStringAsync(address, value).ConfigureAwait(false);
        }

        throw new NotImplementedException();
    }
}
