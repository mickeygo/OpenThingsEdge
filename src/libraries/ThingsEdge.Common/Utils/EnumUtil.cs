namespace ThingsEdge.Common.Utils;

/// <summary>
/// 枚举帮助类
/// </summary>
public static class EnumUtil
{
    /// <summary>
    /// 提取枚举类型中定义的字段集合，字段常量为 key, 自动名称为 value。
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="showDisplayNameAttr">字段名称是否优先显示定义的 <see cref="DisplayAttribute.Name"/> 值，没有设置或为空则返回枚举自身。</param>
    /// <returns></returns>
    public static Dictionary<int, string> FetchDictionary<TEnum>(bool showDisplayNameAttr = false)
       where TEnum : Enum
    {
        var fileds = typeof(TEnum).GetFields(BindingFlags.Static | BindingFlags.Public); // 排除内置的 "value__" 字段
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
    /// 提取枚举类型中定义的字段集合。
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="showDisplayNameAttr">字段名称是否优先显示定义的 <see cref="DisplayAttribute.Name"/> 值，没有设置或为空则返回枚举自身。</param>
    /// <returns></returns>
    public static IEnumerable<string> FetchStrings<TEnum>(bool showDisplayNameAttr = false)
      where TEnum : Enum
    {
        var fileds = typeof(TEnum).GetFields(BindingFlags.Static | BindingFlags.Public); // 排除内置的 "value__" 字段
        foreach (var field in fileds)
        {
            if (showDisplayNameAttr)
            {
                var attr = field.GetCustomAttribute<DisplayAttribute>();
                if (!string.IsNullOrEmpty(attr?.Name))
                {
                    yield return attr.Name;
                }
            }
            
            yield return field.Name;
        }
    }
}
