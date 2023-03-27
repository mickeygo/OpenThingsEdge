namespace Ops.Contrib.Kepware.IotGateway.RESTful;

/// <summary>
/// RESTful 服务端参数选项。
/// </summary>
public sealed class RESTfulServerOptions
{
    /// <summary>
    /// REST 服务端基地址。
    /// </summary>
    [NotNull]
    public string? RESTServerBaseAddress { get; set; }

    /// <summary>
    /// 是否允许匿名访问，默认为true。
    /// </summary>
    /// <remarks>注：当不允许匿名登录是，必须使用验证凭证。</remarks>
    public bool AllowAnonymous { get; set; } = true;

    /// <summary>
    /// 凭证路径。
    /// </summary>
    public string? CertificatePath { get; set; }

    /// <summary>
    /// 是否禁用凭证检测（https），默认为 true。
    /// </summary>
    public bool DisableCertificateValidationCheck { get; set; } = true;

}
