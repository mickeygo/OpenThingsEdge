namespace ThingsEdge.Common.Utils;

public static class EnumExtensions
{
    /// <summary>
    /// 获取 <see cref="Enum"/> 设定的 <see cref="DisplayAttribute.Name"/> 值，没有设置或为空则返回枚举自身。
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string DisplayName(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attr = field!.GetCustomAttribute<DisplayAttribute>();
        if (!string.IsNullOrEmpty(attr?.Name))
        {
            return attr.Name;
        }

        return field!.Name;
    }
}
