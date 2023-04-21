namespace ThingsEdge.Router.Configuration;

/// <summary>
/// Forwarder 常量。
/// </summary>
internal static class ForwarderConstants
{
    /// <summary>
    /// HttpClinet 名称。
    /// </summary>
    internal const string HttpClientName = "ThingsEdge.Router.RESTfulClient";

    /// <summary>
    /// 目标服务数据接收 API 地址前缀。
    /// </summary>
    internal const string RequestUri = "/api/iotgateway";

    /// <summary>
    /// 目标服务器健康检测地址。
    /// </summary>
    internal const string HealthRequestUri = "/api/health";
}
