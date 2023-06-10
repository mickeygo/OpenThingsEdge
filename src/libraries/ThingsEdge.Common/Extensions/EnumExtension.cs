namespace ThingsEdge.Common.Extensions;

public static class EnumExtension
{
    public static Dictionary<int, string> ToDictionary(this Enum value, bool showDisplayNameAttr = false)
    {
        var fileds = value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public);
        Dictionary<int, string> map = new(fileds.Length);
        foreach (var field in fileds)
        {
            string v = field.Name;
            if (showDisplayNameAttr)
            {
                var attr = field.GetCustomAttribute<DisplayAttribute>();
                if (!string.IsNullOrEmpty(attr?.Name))
                {
                    v = attr.Name;
                }
            }

            int k = (int)field.GetRawConstantValue()!;
            map[k] = v;
        }

        return map;
    }

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
