namespace Ops.Contrib.Kepware.IotGateway.RESTful;

/// <summary>
/// RESTful 客户端参数选项。
/// </summary>
public sealed class RESTfulClientOptions
{
    /// <summary>
    /// REST 客户端基地址。
    /// </summary>
    [NotNull]
    public string? RESTClientBaseAddress { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    /// <summary>
    /// 是否禁用凭证检测（https），默认为 true。
    /// </summary>
    public bool DisableCertificateValidationCheck { get; set; } = true;
}
