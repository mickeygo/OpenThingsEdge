namespace ThingsEdge.Contracts.Utils;

public static class ObjectConvertUtil
{
    /// <summary>
    /// 将 object 对象强制转换为数组。若对象不为集合类型，或是不能转换为指定类型，会抛出异常。
    /// </summary>
    /// <remarks>
    /// 可转换的参数类型：<see cref="bool"/>、<see cref="byte"/>、<see cref="ushort"/>、<see cref="short"/>、
    /// <see cref="uint"/>、<see cref="int"/>、<see cref="float"/>、<see cref="double"/>。
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static T[] ToArray<T>(object obj)
        where T : struct
    {
        if (obj is T[] obj2)
        {
            return obj2;
        }

        if (obj is IEnumerable<T> obj3)
        {
            return obj3.ToArray();
        }

        if (obj is not IEnumerable enumerable)
        {
            throw new FormatException("被转换的对象必须为枚举类型");
        }

        List<object> result = new();
        foreach (var item in enumerable)
        {
            if (typeof(T) == typeof(bool))
            {
                result.Add(Convert.ToBoolean(item));
            }
            else if (typeof(T) == typeof(byte))
            {
                result.Add(Convert.ToByte(item));
            }
            else if (typeof(T) == typeof(ushort))
            {
                result.Add(Convert.ToUInt16(item));
            }
            else if (typeof(T) == typeof(short))
            {
                result.Add(Convert.ToInt16(item));
            }
            else if (typeof(T) == typeof(uint))
            {
                result.Add(Convert.ToUInt32(item));
            }
            else if (typeof(T) == typeof(int))
            {
                result.Add(Convert.ToInt32(item));
            }
            else if (typeof(T) == typeof(float))
            {
                result.Add(Convert.ToSingle(item));
            }
            else if (typeof(T) == typeof(double))
            {
                result.Add(Convert.ToDouble(item));
            }
            else
            {
                throw new InvalidOperationException("转换类型必须为 bool、byte、ushort、short、uint、int、float 和 double 中的一种");
            }
        }
        return result.OfType<T>().ToArray();
    }
}
