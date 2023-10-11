using System.ComponentModel;

namespace ThingsEdge.Common.Utils;

public static class EnumExtensions
{
    /// <summary>
    /// 获取 <see cref="Enum"/> 设定的 <see cref="DescriptionAttribute"/> 值，没有设置或为空则返回枚举自身。
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string Description(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attr = field!.GetCustomAttribute<DescriptionAttribute>();
        if (!string.IsNullOrEmpty(attr?.Description))
        {
            return attr.Description;
        }

        return field!.Name;
    }
}
