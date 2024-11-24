using ThingsEdge.Exchange.Contracts;
using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Engine.Policies;

/// <summary>
/// 默认开关监控策略
/// </summary>
internal sealed class DefaultSwitchMonitorPolicy : IMonitorPolicy
{
    public bool Validate(PayloadData data)
    {
        if (data.IsArray())
        {
            throw new InvalidOperationException("Tag 数据类型不能为数组");
        }

        return data.DataType switch
        {
            TagDataType.Bit => data!.GetBit(),
            TagDataType.Byte => data.GetByte() == 1,
            TagDataType.Word => data.GetWord() == 1,
            TagDataType.Int => data!.GetInt() == 1,
            _ => throw new InvalidOperationException("Tag 数据类型必须为 bool 或 short。"),
        };
    }

    public object? Return(Tag tag)
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
            _ => throw new InvalidOperationException("Tag 数据类型必须为 bool 或 short。"),
        };

        var data = PayloadData.FromTag(tag);
        data.Value = obj;

        return data;
    }
}
