namespace ThingsEdge.Communication.Common.Serial;

/// <summary>
/// 用于LRC验证的类，提供了标准的验证方法。
/// </summary>
public static class SoftLRC
{
    /// <summary>
    /// 获取对应的数据的LRC校验码。
    /// </summary>
    /// <param name="value">需要校验的数据，不包含LRC字节</param>
    /// <returns>返回带LRC校验码的字节数组，可用于串口发送</returns>
    public static byte[] LRC(byte[] value)
    {
        var num = 0;
        for (var i = 0; i < value.Length; i++)
        {
            num += value[i];
        }
        num %= 256;
        num = 256 - num;
        var array = new byte[1] { (byte)num };
        return CollectionUtils.SpliceArray(value, array);
    }

    /// <summary>
    /// 获取对应的数据的LRC校验码。
    /// </summary>
    /// <param name="value">需要校验的数据，不包含LRC字节</param>
    /// <param name="left">忽略的左边的字节数量</param>
    /// <param name="right">忽略的右边的字节数量</param>
    /// <returns>返回LRC校验码</returns>
    public static byte LRC(byte[] value, int left, int right)
    {
        var num = 0;
        for (var i = left; i < value.Length - right; i++)
        {
            num += value[i];
        }
        num %= 256;
        num = 256 - num;
        return (byte)num;
    }

    /// <summary>
    /// 检查数据是否符合LRC的验证。
    /// </summary>
    /// <param name="value">等待校验的数据，是否正确</param>
    /// <returns>是否校验成功</returns>
    public static bool CheckLRC(byte[] value)
    {
        if (value.Length == 0)
        {
            return false;
        }

        var num = value.Length;
        var array = new byte[num - 1];
        Array.Copy(value, 0, array, 0, array.Length);
        var array2 = LRC(array);
        if (array2[num - 1] == value[num - 1])
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 根据传入的原始字节数组，计算和校验信息，可以指定起始的偏移地址和尾部的字节数量信息。
    /// </summary>
    /// <param name="buffer">原始字节数组信息</param>
    /// <returns>和校验的结果</returns>
    public static int CalculateAcc(byte[] buffer)
    {
        return CalculateAcc(buffer, 0, 0);
    }

    /// <summary>
    /// 根据传入的原始字节数组，计算和校验信息，可以指定起始的偏移地址和尾部的字节数量信息。
    /// </summary>
    /// <param name="buffer">原始字节数组信息</param>
    /// <param name="headCount">起始的偏移地址信息</param>
    /// <param name="lastCount">尾部的字节数量信息</param>
    /// <returns>和校验的结果</returns>
    public static int CalculateAcc(byte[] buffer, int headCount, int lastCount)
    {
        var num = 0;
        for (var i = headCount; i < buffer.Length - lastCount; i++)
        {
            num += buffer[i];
        }
        return num;
    }

    /// <summary>
    /// 计算数据的和校验，并且输入和校验的值信息。
    /// </summary>
    /// <param name="buffer">原始字节数组信息</param>
    /// <param name="headCount">起始的偏移地址信息</param>
    /// <param name="lastCount">尾部的字节数量信息</param>
    public static void CalculateAccAndFill(byte[] buffer, int headCount, int lastCount)
    {
        var b = (byte)CalculateAcc(buffer, headCount, lastCount);
        Encoding.ASCII.GetBytes(b.ToString("X2")).CopyTo(buffer, buffer.Length - lastCount);
    }

    /// <summary>
    /// 计算数据的和校验，并且和当前已经存在的和校验信息进行匹配，返回是否匹配成功。
    /// </summary>
    /// <param name="buffer">原始字节数组信息</param>
    /// <param name="headCount">起始的偏移地址信息</param>
    /// <param name="lastCount">尾部的字节数量信息</param>
    /// <returns>和校验是否检查通过</returns>
    public static bool CalculateAccAndCheck(byte[] buffer, int headCount, int lastCount)
    {
        return ((byte)CalculateAcc(buffer, headCount, lastCount)).ToString("X2") == Encoding.ASCII.GetString(buffer, buffer.Length - lastCount, 2);
    }

    /// <summary>
    /// 计算数据的异或信息，也称为 FCS，可以指定前面无用的字节数量，以及尾部无用的字节数量
    /// </summary>
    /// <param name="source">数据源信息</param>
    /// <param name="left">前面无用的字节数量</param>
    /// <param name="right">后面无用的字节数量</param>
    /// <returns>返回异或校验后的值</returns>
    public static byte[] CalculateFcs(byte[] source, int left, int right)
    {
        int num = source[left];
        for (var i = left + 1; i < source.Length - right; i++)
        {
            num ^= source[i];
        }
        return
        [
            ByteExtensions.BuildAsciiBytesFrom((byte)num)[0],
            ByteExtensions.BuildAsciiBytesFrom((byte)num)[1]
        ];
    }
}
