namespace Ops.Contrib.Kepware.IotGateway.RESTful;

/// <summary>
/// IoTGateway 写入数据的结果。
/// </summary>
public sealed class WriteResultCollection
{
    [NotNull]
    public List<WriteResult>? WriteResults { get; set; }

    public bool IsOk(string tag)
    {
        return WriteResults.FirstOrDefault(s => s.Id == tag)?.S == true;
    }
}

/// <summary>
/// IoTGateway 写入数据的结果。
/// </summary>
public sealed class WriteResult
{
    /// <summary>
    /// 要写入的 Tag。
    /// </summary>
    [NotNull]
    public string? Id { get; set; }

    /// <summary>
    /// 值是否写入成功。
    /// </summary>
    public bool S { get; set; }

    /// <summary>
    /// 写入失败的原因。
    /// </summary>
    [NotNull]
    public string? R { get; set; }
}
