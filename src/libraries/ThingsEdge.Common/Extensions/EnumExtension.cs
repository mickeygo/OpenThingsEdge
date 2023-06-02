using System.ComponentModel.DataAnnotations;

namespace ThingsEdge.Common.Extensions;

public static class EnumExtension
{
    /// <summary>
    /// 获取 <see cref="Enum"/> 设定的 <see cref="DisplayAttribute.Name"/> 值，没有设置或为空则返回枚举自身。
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string DisplayName(this Enum value)
    {
        var attr = value.GetAttribute<DisplayAttribute>();
        if (!string.IsNullOrEmpty(attr?.Name))
        {
            return attr.Name;
        }

        return value.ToString();
    }

    private static TAttribute? GetAttribute<TAttribute>(this Enum value)
        where TAttribute : Attribute
    {
        return value.GetType().GetField(value.ToString())!.GetCustomAttribute<TAttribute>();
    }
}
