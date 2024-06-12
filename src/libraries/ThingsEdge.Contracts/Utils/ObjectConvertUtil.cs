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
    public static bool[] ToBooleanArray(object obj)
    {
        var (ok, arr) = ToReal<bool>(obj);
        return ok ? arr! : ToConvert(obj, Convert.ToBoolean);
    }

    /// <summary>
    /// 转换为 <see cref="byte"/> 类型数组。
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static byte[] ToByteArray(object obj)
    {
        var (ok, arr) = ToReal<byte>(obj);
        return ok ? arr! : ToConvert(obj, Convert.ToByte);
    }

    /// <summary>
    /// 转换为 <see cref="ushort"/> 类型数组。
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static ushort[] ToUInt16Array(object obj)
    {
        var (ok, arr) = ToReal<ushort>(obj);
        return ok ? arr! : ToConvert(obj, Convert.ToUInt16);
    }

    /// <summary>
    /// 转换为 <see cref="short"/> 类型数组。
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static short[] ToInt16Array(object obj)
    {
        var (ok, arr) = ToReal<short>(obj);
        return ok ? arr! : ToConvert(obj, Convert.ToInt16);
    }

    /// <summary>
    /// 转换为 <see cref="uint"/> 类型数组。
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static uint[] ToUInt32Array(object obj)
    {
        var (ok, arr) = ToReal<uint>(obj);
        return ok ? arr! : ToConvert(obj, Convert.ToUInt32);
    }

    /// <summary>
    /// 转换为 <see cref="int"/> 类型数组。
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static int[] ToInt32Array(object obj)
    {
        var (ok, arr) = ToReal<int>(obj);
        return ok ? arr! : ToConvert(obj, Convert.ToInt32);
    }

    /// <summary>
    /// 转换为 <see cref="float"/> 类型数组。
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static float[] ToSingleArray(object obj)
    {
        var (ok, arr) = ToReal<float>(obj);
        return ok ? arr! : ToConvert(obj, Convert.ToSingle);
    }

    /// <summary>
    /// 转换为 <see cref="double"/> 类型数组。
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static double[] ToDoubleArray(object obj)
    {
        var (ok, arr) = ToReal<double>(obj);
        return ok ? arr! : ToConvert(obj, Convert.ToDouble);
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

    private static T[] ToConvert<T>(object obj, Func<object?, T> func)
    {
        if (obj is not IEnumerable enumerable)
        {
            throw new FormatException("被转换的对象必须为可枚举类型");
        }

        List<T> result = [];
        foreach (var item in enumerable)
        {
            result.Add(func(item));
        }

        return [.. result];
    }
}
