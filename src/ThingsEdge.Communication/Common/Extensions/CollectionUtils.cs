namespace ThingsEdge.Communication.Common.Extensions;

internal static class CollectionUtils
{
    /// <summary>
    /// 将一个数组进行扩充到指定长度，或是缩短到指定长度。
    /// </summary>
    /// <typeparam name="T">数组的类型</typeparam>
    /// <param name="array">原先数据的数据</param>
    /// <param name="length">新数组的长度</param>
    /// <returns>新数组长度信息</returns>
    public static T[] ExpandToLength<T>(T[] array, int length)
    {
        if (array == null)
        {
            return new T[length];
        }
        if (array.Length == length)
        {
            return array;
        }

        var array2 = new T[length];
        Array.Copy(array, array2, Math.Min(array.Length, array2.Length));
        return array2;
    }

    /// <summary>
    /// 将一个数组进行扩充到偶数长度。
    /// </summary>
    /// <typeparam name="T">数组的类型</typeparam>
    /// <param name="array">原先数据的数据</param>
    /// <returns>新数组长度信息</returns>
    public static T[] ExpandToEvenLength<T>(T[] array)
    {
        if (array == null)
        {
            return [];
        }
        if (array.Length % 2 == 1)
        {
            return ExpandToLength(array, array.Length + 1);
        }
        return array;
    }

    /// <summary>
    /// 将指定的数据按照指定长度进行分割，例如 int[10]，指定长度4，就分割成 int[4],int[4],int[2]，然后拼接 list。
    /// </summary>
    /// <typeparam name="T">数组的类型</typeparam>
    /// <param name="array">等待分割的数组</param>
    /// <param name="length">指定的长度信息</param>
    /// <returns>分割后结果内容</returns>
    public static List<T[]> SplitByLength<T>(T[] array, int length)
    {
        if (array == null)
        {
            return [];
        }

        var list = new List<T[]>();
        var num = 0;
        while (num < array.Length)
        {
            if (num + length < array.Length)
            {
                var array2 = new T[length];
                Array.Copy(array, num, array2, 0, length);
                num += length;
                list.Add(array2);
            }
            else
            {
                var array3 = new T[array.Length - num];
                Array.Copy(array, num, array3, 0, array3.Length);
                num += length;
                list.Add(array3);
            }
        }
        return list;
    }

    /// <summary>
    /// 创建一个数组，数组长度为 integer / everyLen + mod，并指定每个元素的最大值。
    /// </summary>
    /// <param name="integer">整数信息</param>
    /// <param name="everyLen">单个的数组长度</param>
    /// <returns>拆分后的数组长度</returns>
    public static int[] SplitIntegerToArray(int integer, int everyLen)
    {
        var mod = integer % everyLen;
        var array = new int[integer / everyLen + (mod != 0 ? 1 : 0)];
        for (var i = 0; i < array.Length; i++)
        {
            if (i == array.Length - 1)
            {
                array[i] = mod == 0 ? everyLen : mod;
            }
            else
            {
                array[i] = everyLen;
            }
        }
        return array;
    }

    /// <summary>
    /// 切割当前的地址数据信息，根据读取的长度来分割成多次不同的读取内容，需要指定地址，总的读取长度，切割读取长度。
    /// </summary>
    /// <param name="address">整数的地址信息</param>
    /// <param name="length">读取长度信息</param>
    /// <param name="segment">切割长度信息</param>
    /// <returns>切割结果</returns>
    public static (int[], int[]) SplitReadLength(int address, int length, int segment)
    {
        var array = SplitIntegerToArray(length, segment);
        var array2 = new int[array.Length];
        for (var i = 0; i < array2.Length; i++)
        {
            if (i == 0)
            {
                array2[i] = address;
            }
            else
            {
                array2[i] = array2[i - 1] + array[i - 1];
            }
        }
        return (array2, array);
    }

    /// <summary>
    /// 拼接任意个泛型数组为一个总的泛型数组对象，采用深度拷贝实现。
    /// </summary>
    /// <typeparam name="T">数组的类型信息</typeparam>
    /// <param name="arrays">任意个长度的数组</param>
    /// <returns>拼接之后的最终的结果对象</returns>
    public static T[] SpliceArray<T>(params T[][] arrays)
    {
        var num = 0;
        for (var i = 0; i < arrays.Length; i++)
        {
            var obj = arrays[i];
            if (obj != null && obj.Length != 0)
            {
                num += arrays[i].Length;
            }
        }

        var num2 = 0;
        var array = new T[num];
        for (var j = 0; j < arrays.Length; j++)
        {
            var obj2 = arrays[j];
            if (obj2 != null && obj2.Length != 0)
            {
                arrays[j].CopyTo(array, num2);
                num2 += arrays[j].Length;
            }
        }
        return array;
    }
}
