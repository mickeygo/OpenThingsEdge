using ThingsEdge.Exchange.Contracts;

namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 开关数据上下文。
/// </summary>
/// <param name="Message">请求消息</param>
/// <param name="CurveName">曲线名称</param>
/// <param name="Masters">主数据，顺序与按配置文件一致</param>
/// <param name="FilePath">本地文件路径</param>
public sealed record SwitchContext(
    RequestMessage Message,
    string? CurveName,
    IReadOnlyList<PayloadData> Masters,
    string? FilePath);
