namespace ThingsEdge.Router.Configuration;

/// <summary>
/// 基于 RESTful 格式的目标服务参数选项。
/// </summary>
public sealed class RESTfulDestinationOptions
{
    /// <summary>
    /// 请求服务基地址。
    /// </summary>
    [NotNull]
    public string? BaseAddress { get; set; }

    /// <summary>
    /// 目标服务是否启用 Basic Authentication 验证。
    /// </summary>
    public bool EnableBasicAuth { get; set; }

    /// <summary>
    /// 验证使用的用户名。
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 验证使用的密码。
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 是否禁用凭证检测（https），默认为 true。
    /// </summary>
    public bool DisableCertificateValidationCheck { get; set; } = true;
}
