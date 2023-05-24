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
    /// 请求目标超时设置，单位毫秒。
    /// </summary>
    public int? Timeout { get; set; } = 10_000;

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

    /// <summary>
    /// 目标服务健康检测地址，默认为 "/api/health"。
    /// </summary>
    public string? HealthRequestUrl { get; set; } = "/api/health";

    /// <summary>
    /// 目标服务接收通知数据的地址，默认为 "/api/iotgateway/notice"。
    /// </summary>
    public string? NoticeRequestUrl { get; set; } = "/api/iotgateway/notice";

    /// <summary>
    /// 目标服务接收触发数据的地址，默认为 "/api/iotgateway/trigger"。
    /// </summary>
    public string? TriggerRequestUrl { get; set; } = "/api/iotgateway/trigger";
}
