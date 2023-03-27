namespace Ops.Contrib.Kepware.IotGateway.RESTful;

/// <summary>
/// IoTGateway 读取数据结果。
/// </summary>
public sealed class ReadResult
{
    /// <summary>
    /// 读取的数据集合。
    /// </summary>
    [NotNull]
    public List<ReadData>? ReadResults { get; set; }
}

/// <summary>
/// IoTGateway 读取的数据。
/// </summary>
public sealed class ReadData
{
    /// <summary>
    /// Tag。
    /// <para>注：Tag 不存在时也能读取出数据，但结果会为 false。</para>
    /// </summary>
    [NotNull]
    public string? Id { get; set; }

    /// <summary>
    /// 数据读取结果。
    /// </summary>
    public bool S { get; set; }

    /// <summary>
    /// 数据读取失败原因，但未失败是为 ""。
    /// </summary>
    [NotNull]
    public string? R { get; set; }

    /// <summary>
    /// 读取的数据，根据实际转换为对应的数据。
    /// </summary>
    [NotNull]
    public object? V { get; set; }

    /// <summary>
    /// 数据更新的时间戳。
    /// </summary>
    public long T { get; set; }

    #region 获取具体的数据

    public (bool ok, bool value) GetBoolean()
    {
        if (V is bool v1)
        {
            return (true, v1);
        }

        return (false, false);
    }

    public (bool ok, bool[] value) GetArrayBoolean()
    {
        throw new NotImplementedException();
    }

    public (bool ok, byte value) GetByte()
    {
        throw new NotImplementedException();
    }

    public (bool ok, byte[] value) GetArrayByte()
    {
        throw new NotImplementedException();
    }

    public (bool ok, int value) GetInt32()
    {
        if (V is int v1)
        {
            return (true, v1);
        }

        return (false, 0);
    }

    public (bool ok, int[] value) GetArrayInt32()
    {
        throw new NotImplementedException();
    }

    public (bool ok, long value) GetInt64()
    {
        if (V is long v1)
        {
            return (true, v1);
        }

        return (false, 0);
    }

    public (bool ok, long[] value) GetArrayInt64()
    {
        throw new NotImplementedException();
    }

    public (bool ok, double value) GetDouble()
    {
        if (V is double v1)
        {
            return (true, v1);
        }
       
        return (false, 0);
    }

    public (bool ok, double[] value) GetArrayDouble()
    {
        throw new NotImplementedException();
    }

    public (bool ok, string value) GetString()
    {
        if (V is string v1)
        {
            return (true, v1);
        }

        return (true, V.ToString() ?? "");
    }

    #endregion
}
