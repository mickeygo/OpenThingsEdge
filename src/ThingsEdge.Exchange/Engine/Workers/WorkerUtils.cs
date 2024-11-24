using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Contracts;

namespace ThingsEdge.Exchange.Engine.Workers;

/// <summary>
/// 帮助类。
/// </summary>
internal static class WorkerUtils
{
    /// <summary>
    /// 检查数据是否处于 On 状态。
    /// </summary>
    /// <remarks>数据类型必须为 bool、byte、ushort 或 short 类型，不为数组。</remarks>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static bool CheckOn(PayloadData data)
    {
        if (data.IsArray())
        {
            throw new InvalidOperationException("Tag 数据类型不能为数组");
        }

        return data.DataType switch
        {
            TagDataType.Bit => data.GetBit(),
            TagDataType.Byte => data.GetByte() == 1,
            TagDataType.Word => data.GetWord() == 1,
            TagDataType.Int => data.GetInt() == 1,
            _ => throw new InvalidOperationException("Tag 数据类型必须为 bool、byte、ushort 或 short。"),
        };
    }

    /// <summary>
    /// 设置标记为 Off 状态，bool 类型设置为 false, 数值类型设置为 0。
    /// </summary>
    /// <remarks>数据类型必须为 bool、byte、ushort 或 short 类型，不为数组。</remarks>
    /// <param name="tag"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static PayloadData SetOff(Tag tag)
    {
        var obj = SetOff2(tag);
        var data = PayloadData.FromTag(tag);
        data.Value = obj;
        return data;
    }

    /// <summary>
    /// 设置标记为 Off 状态，bool 类型设置为 false, 数值类型设置为 0。
    /// </summary>
    /// <remarks>数据类型必须为 bool、byte、ushort 或 short 类型，不为数组。</remarks>
    /// <param name="tag"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static object SetOff2(Tag tag)
    {
        if (tag.IsArray())
        {
            throw new InvalidOperationException("Tag 数据类型不能为数组");
        }

        return tag.DataType switch
        {
            TagDataType.Bit => false,
            TagDataType.Byte => (byte)0,
            TagDataType.Word => (ushort)0,
            TagDataType.Int => (short)0,
            _ => throw new InvalidOperationException("Tag 数据类型必须为 bool、byte、ushort 或 short。"),
        };
    }
}
