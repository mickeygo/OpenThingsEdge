namespace ThingsEdge.Contracts.Utils;

/// <summary>
/// object 对象转换帮助类。
/// </summary>
public static class ObjectConvertUtil
{
    /// <summary>
    /// 转换为 <see cref="bool"/> 类型。
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static bool[] ToBooleanArray(object obj) => ToArray(obj, Convert.ToBoolean);

    /// <summary>
    /// 转换为 <see cref="byte"/> 类型数组。
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static byte[] ToByteArray(object obj) => ToArray(obj, Convert.ToByte);

    /// <summary>
    /// 转换为 <see cref="ushort"/> 类型数组。
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static ushort[] ToUInt16Array(object obj) => ToArray(obj, Convert.ToUInt16);

    /// <summary>
    /// 转换为 <see cref="short"/> 类型数组。
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static short[] ToInt16Array(object obj) => ToArray(obj, Convert.ToInt16);

    /// <summary>
    /// 转换为 <see cref="uint"/> 类型数组。
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static uint[] ToUInt32Array(object obj) => ToArray(obj, Convert.ToUInt32);

    /// <summary>
    /// 转换为 <see cref="int"/> 类型数组。
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static int[] ToInt32Array(object obj) => ToArray(obj, Convert.ToInt32);

    /// <summary>
    /// 转换为 <see cref="float"/> 类型数组。
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static float[] ToSingleArray(object obj) => ToArray(obj, Convert.ToSingle);

    /// <summary>
    /// 转换为 <see cref="double"/> 类型数组。
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static double[] ToDoubleArray(object obj) => ToArray(obj, Convert.ToDouble);

    /// <summary>
    /// 将对象转换为指定类型的数据。
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    /// <param name="obj"></param>
    /// <param name="convert">使用 System.Convert 进行转换</param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static TTarget[] ToArray<TTarget>(object obj, Func<object?, TTarget> convert)
        where TTarget : struct
    {
        var (ok, arr) = ToReal<TTarget>(obj);
        return ok ? arr! : ToConvert(obj, convert);
    }

    private static (bool, T[]?) ToReal<T>(object obj)
    {
        if (obj is T[] obj2)
        {
            return (true, obj2);
        }

        if (obj is IEnumerable<T> obj3)
        {
            return (true, [.. obj3]);
        }

        return (false, default);
    }

    private static T[] ToConvert<T>(object obj, Func<object?, T> convert)
    {
        if (obj is not IEnumerable enumerable)
        {
            throw new FormatException("被转换的对象必须为可枚举类型");
        }

        List<T> result = [];
        foreach (var item in enumerable)
        {
            if (item is T item2)
            {
                result.Add(item2);
            }
            else
            {
                result.Add(convert(item));
            }
        }

        return [.. result];
    }
}
