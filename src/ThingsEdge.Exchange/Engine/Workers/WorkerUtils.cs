using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Contracts;

namespace ThingsEdge.Exchange.Engine.Workers;

/// <summary>
/// 帮助类。
/// </summary>
internal static class WorkerUtils
{
    /// <summary>
    /// 检查心跳数据是否处于 On 状态，高电平时为 true 或 1，低电平时为 false 或 0。
    /// </summary>
    /// <remarks>数据类型必须为 bool、byte、ushort 或 short 类型，不能为数组。</remarks>
    /// <param name="data"></param>
    /// <param name="useHighLevel">是否使用高电平</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static bool CheckHeartbeatOn(PayloadData data, bool useHighLevel)
    {
        if (data.IsArray())
        {
            throw new InvalidOperationException("Tag 数据类型不能为数组");
        }

        return data.DataType switch
        {
            TagDataType.Bit => data.GetBit() == useHighLevel,
            TagDataType.Byte => data.GetByte() == (useHighLevel ? (byte)1 : (byte)0),
            TagDataType.Word => data.GetWord() == (useHighLevel ? (ushort)1 : (ushort)0),
            TagDataType.Int => data.GetInt() == (useHighLevel ? (short)1 : (short)0),
            _ => throw new InvalidOperationException("Tag 数据类型必须为 bool、byte、ushort 或 short。"),
        };
    }

    /// <summary>
    /// 检查开关信号是否处于 On 状态。
    /// </summary>
    /// <remarks>数据类型必须为 bool、byte、ushort 或 short 类型，不能为数组。</remarks>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static bool CheckSwitchOn(PayloadData data)
    {
        if (data.IsArray())
        {
            throw new InvalidOperationException("Tag 数据类型不能为数组");
        }

        return data.DataType switch
        {
            TagDataType.Bit => data.GetBit() == true,
            TagDataType.Byte => data.GetByte() == 1,
            TagDataType.Word => data.GetWord() == 1,
            TagDataType.Int => data.GetInt() == 1,
            _ => throw new InvalidOperationException("Tag 数据类型必须为 bool、byte、ushort 或 short。"),
        };
    }

    /// <summary>
    /// 根据心跳 Tag 创建一个 off 状态的 PayloadData，其中值在高电平时为 false 或 0，低电平时为 true 或 1。
    /// </summary>
    /// <remarks>数据类型必须为 bool、byte、ushort 或 short 类型，不能为数组。</remarks>
    /// <param name="tag"></param>
    /// <param name="useHighLevel">是否使用高电平</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static PayloadData CreateHeartbeatPayloadOff(Tag tag, bool useHighLevel)
    {
        var obj = SetHeartbeatOff(tag, useHighLevel);
        var data = PayloadData.FromTag(tag);
        data.Value = obj;
        return data;
    }

    /// <summary>
    /// 设置心跳标记为 Off 状态，高电平时设为 false 或 0 ，低电平时设为 true 或 1。
    /// </summary>
    /// <remarks>数据类型必须为 bool、byte、ushort 或 short 类型，不能为数组。</remarks>
    /// <param name="tag"></param>
    /// <param name="useHighLevel">是否使用高电平</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static object SetHeartbeatOff(Tag tag, bool useHighLevel)
    {
        if (tag.IsArray())
        {
            throw new InvalidOperationException("Tag 数据类型不能为数组");
        }

        return tag.DataType switch
        {
            TagDataType.Bit => useHighLevel,
            TagDataType.Byte => useHighLevel ? (byte)0 : (byte)1,
            TagDataType.Word => useHighLevel ? (ushort)0 : (ushort)1,
            TagDataType.Int => useHighLevel ? (short)0 : (short)1,
            _ => throw new InvalidOperationException("Tag 数据类型必须为 bool、byte、ushort 或 short。"),
        };
    }

    /// <summary>
    /// 根据开关 Tag 创建一个 off 状态的 PayloadData。
    /// </summary>
    /// <remarks>数据类型必须为 bool、byte、ushort 或 short 类型，不能为数组。</remarks>
    /// <param name="tag"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static PayloadData CreateSwitchPayloadOffOff(Tag tag)
    {
        if (tag.IsArray())
        {
            throw new InvalidOperationException("Tag 数据类型不能为数组");
        }

        object obj = tag.DataType switch
        {
            TagDataType.Bit => false,
            TagDataType.Byte => (byte)0,
            TagDataType.Word => (ushort)0,
            TagDataType.Int => (short)0,
            _ => throw new InvalidOperationException("Tag 数据类型必须为 bool、byte、ushort 或 short。"),
        };
        var data = PayloadData.FromTag(tag);
        data.Value = obj;
        return data;
    }

    /// <summary>
    /// 获取触发标记的值。
    /// </summary>
    /// <remarks>数据类型必须为 short 或 int 类型，不能为数组。</remarks>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static int GetTriggerState(PayloadData data)
    {
        if (data.IsArray())
        {
            throw new InvalidOperationException("Tag 数据类型不能为数组");
        }

        return data.DataType switch
        {
            TagDataType.Int => data.GetInt(),
            TagDataType.DInt => data.GetDInt(),
            _ => throw new InvalidOperationException("Tag 数据类型必须为 short 或 int。"),
        };
    }
}
