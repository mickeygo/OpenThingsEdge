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
    /// <param name="showDescriptionAttr">字段名称是否优先显示定义的 <see cref="DescriptionAttribute"/> 值，没有设置或为空则返回枚举自身。</param>
    /// <returns></returns>
    public static Dictionary<int, string> FetchDictionary<TEnum>(bool showDescriptionAttr = false)
       where TEnum : Enum
    {
        var fileds = typeof(TEnum).GetFields(BindingFlags.Static | BindingFlags.Public); // 排除内置的 "value__" 字段
        Dictionary<int, string> map = new(fileds.Length);
        foreach (var field in fileds)
        {
            string v = field.Name;
            if (showDescriptionAttr)
            {
                var attr = field.GetCustomAttribute<DescriptionAttribute>(false);
                if (!string.IsNullOrEmpty(attr?.Description))
                {
                    v = attr.Description;
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
    /// <param name="showDescriptionAttr">字段名称是否优先显示定义的 <see cref="DescriptionAttribute"/> 值，没有设置或为空则返回枚举自身。</param>
    /// <returns></returns>
    public static IEnumerable<string> FetchStrings<TEnum>(bool showDescriptionAttr = false)
      where TEnum : Enum
    {
        var fileds = typeof(TEnum).GetFields(BindingFlags.Static | BindingFlags.Public); // 排除内置的 "value__" 字段
        foreach (var field in fileds)
        {
            if (showDescriptionAttr)
            {
                var attr = field.GetCustomAttribute<DescriptionAttribute>(false);
                if (!string.IsNullOrEmpty(attr?.Description))
                {
                    yield return attr.Description;
                }
            }
            
            yield return field.Name;
        }
    }
}
