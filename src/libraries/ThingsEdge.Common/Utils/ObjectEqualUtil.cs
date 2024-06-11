namespace ThingsEdge.Common.Utils;

/// <summary>
/// 对象比较帮助类。
/// </summary>
public static class ObjectEqualUtil
{
    private static readonly Type[] Types = [typeof(bool), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), 
        typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(double), typeof(decimal)];

    private static readonly Type[] TypeArray = [typeof(bool[]), typeof(byte[]), typeof(sbyte[]), typeof(short[]), typeof(ushort[]), 
        typeof(int[]), typeof(uint[]), typeof(long[]), typeof(ulong[]), typeof(double), typeof(decimal), typeof(string[])];

    /// <summary>
    /// 比较两对象是否相等
    /// </summary>
    /// <remarks>
    /// 此处比较，仅仅比较 bool、sbyte、byte、ushort、short、uint、int、ulong、long、double、decimal 和 string 数据以及其对应的数组值，float 永远不等。
    /// 对于数组值，会比较两者类型。
    /// </remarks>
    /// <param name="actual"></param>
    /// <param name="expected"></param>
    /// <returns></returns>
    public static bool IsEqual(object actual, object expected)
    {
        if (actual == expected) return true;

        Type actualType = actual.GetType();
        Type expectedType = expected.GetType();

        // 单一值
        if (Types.Contains(actualType) && Types.Contains(expectedType))
        {
            return actual.ToString() == expected.ToString(); // 转为字符串，减少麻烦
        }

        // String 单独比较
        if (actual is string actualStr && expected is string expectedStr)
        {
            return actualStr == expectedStr;
        }

        // 数组值
        if (actualType.IsArray && expectedType.IsArray)
        {
            if (!TypeArray.Contains(actualType) || !TypeArray.Contains(expectedType))
            {
                return false;
            }

            if (actual is bool[] boolArray1 && expected is bool[] boolArray2)
            {
                return ArrayEqual(boolArray1, boolArray2);
            }
            if (actual is byte[] byteArray1 && expected is byte[] byteArray2)
            {
                return ArrayEqual(byteArray1, byteArray2);
            }
            if (actual is sbyte[] sbyteArray1 && expected is sbyte[] sbyteArray2)
            {
                return ArrayEqual(sbyteArray1, sbyteArray2);
            }
            if (actual is short[] shortArray1 && expected is short[] shortArray2)
            {
                return ArrayEqual(shortArray1, shortArray2);
            }
            if (actual is ushort[] ushortArray1 && expected is ushort[] ushortArray2)
            {
                return ArrayEqual(ushortArray1, ushortArray2);
            }
            if (actual is int[] intArray1 && expected is int[] intArray2)
            {
                return ArrayEqual(intArray1, intArray2);
            }
            if (actual is uint[] uintArray1 && expected is uint[] uintArray2)
            {
                return ArrayEqual(uintArray1, uintArray2);
            }
            if (actual is long[] longArray1 && expected is long[] longArray2)
            {
                return ArrayEqual(longArray1, longArray2);
            }
            if (actual is ulong[] ulongArray1 && expected is ulong[] ulongArray2)
            {
                return ArrayEqual(ulongArray1, ulongArray2);
            }
            if (actual is double[] doubleArray1 && expected is double[] doubleArray2)
            {
                return ArrayEqual(doubleArray1, doubleArray2);
            }
            if (actual is decimal[] decimalArray1 && expected is decimal[] decimaleArray2)
            {
                return ArrayEqual(decimalArray1, decimaleArray2);
            }
            if (actual is string[] strArray1 && expected is string[] strArray2)
            {
                return StringArrayEqual(strArray1, strArray2);
            }
        }

        return false;

        static bool ArrayEqual<T>(T[] t1, T[] t2)
            where T : struct, IEquatable<T>
        {
            if (t1.Length != t2.Length)
            {
                return false;
            }

            for (var i = 0; i < t1.Length; i++)
            {
                if (!t1[i].Equals(t2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        static bool StringArrayEqual(string[] t1, string[] t2)
        {
            if (t1.Length != t2.Length)
            {
                return false;
            }

            for (var i = 0; i < t1.Length; i++)
            {
                if (!t1[i].Equals(t2[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
