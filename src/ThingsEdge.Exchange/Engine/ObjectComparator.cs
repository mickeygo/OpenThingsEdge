namespace ThingsEdge.Exchange.Engine;

/// <summary>
/// 对象比较帮助类。
/// </summary>
internal static class ObjectComparator
{
    /// <summary>
    /// 浮点类型（float、double）进行比较的阈值。
    /// </summary>
    public static double FloatingEpsilon = 0.0001;

    private static readonly Type[] s_types = [typeof(bool), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
        typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(string)];

    private static readonly Type[] s_typeArray = [typeof(bool[]), typeof(byte[]), typeof(sbyte[]), typeof(short[]), typeof(ushort[]),
        typeof(int[]), typeof(uint[]), typeof(long[]), typeof(ulong[]), typeof(float), typeof(double), typeof(decimal), typeof(string[])];

    /// <summary>
    /// 比较两对象是否相等。
    /// </summary>
    /// <remarks>
    /// 此处比较，仅仅比较 bool、sbyte、byte、ushort、short、uint、int、ulong、long、float、double、decimal 和 string 数据以及其对应的数组值，
    /// 其中 float 和 double 会判断两者差值的绝对值要大于阈值。
    /// 对于数组值，会比较两者类型。
    /// </remarks>
    /// <param name="actual">实际值</param>
    /// <param name="expected">期望值</param>
    /// <returns></returns>
    public static bool IsEqual(object actual, object expected)
    {
        // 先直接比较
        if (actual == expected)
        {
            return true;
        }

        var actualType = actual.GetType();
        var expectedType = expected.GetType();

        if (!actualType.IsArray && !expectedType.IsArray)
        {
            // 单一值
            if (s_types.Contains(actualType) && s_types.Contains(expectedType))
            {
                if (actual is bool bool1 && expected is bool bool2)
                {
                    return bool1 == bool2;
                }
                if (actual is byte byte1 && expected is byte byte2)
                {
                    return byte1 == byte2;
                }
                if (actual is sbyte sbyte1 && expected is sbyte sbyte2)
                {
                    return sbyte1 == sbyte2;
                }
                if (actual is short short1 && expected is short short2)
                {
                    return short1 == short2;
                }
                if (actual is ushort ushort1 && expected is ushort ushort2)
                {
                    return ushort1 == ushort2;
                }
                if (actual is int int1 && expected is int int2)
                {
                    return int1 == int2;
                }
                if (actual is uint uint1 && expected is uint uint2)
                {
                    return uint1 == uint2;
                }
                if (actual is long long1 && expected is long long2)
                {
                    return long1 == long2;
                }
                if (actual is ulong ulong1 && expected is ulong ulong2)
                {
                    return ulong1 == ulong2;
                }
                if (actual is float float1 && expected is float float2)
                {
                    return Math.Abs(float1 - float2) > FloatingEpsilon;
                }
                if (actual is double double1 && expected is double double2)
                {
                    return Math.Abs(double1 - double2) > FloatingEpsilon;
                }
                if (actual is decimal decimal1 && expected is decimal decimal2)
                {
                    return decimal1 == decimal2;
                }
                if (actual is string str1 && expected is string str2)
                {
                    return str1 == str2;
                }
            }
        }
        else
        {
            // 数组值
            if (!s_typeArray.Contains(actualType) || !s_typeArray.Contains(expectedType))
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
            if (actual is float[] floatArray1 && expected is float[] floatArray2)
            {
                return ArrayEqual(floatArray1, floatArray2);
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
                if (!ValueEqual(t1[i], t2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        static bool ValueEqual<T>(T t1, T t2)
            where T : IEquatable<T>
        {
            if (typeof(T) == typeof(float) && t1 is float f1 && t2 is float f2)
            {
                return Math.Abs(f1 - f2) > FloatingEpsilon;
            }
            if (typeof(T) == typeof(double) && t1 is double db1 && t2 is double db2)
            {
                return Math.Abs(db1 - db2) > FloatingEpsilon;
            }

            return t1.Equals(t2);
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
