namespace ThingsEdge.Contrib.DotNetty;

public sealed class ServerOptions
{
    public int Port { get; set; }

    public bool UseLibuv { get; set; }

    /// <summary>
    /// 是否启用 SSL 验证。
    /// </summary>
    public bool IsSsl { get; set; }

    /// <summary>
    /// 证书文件路径。
    /// </summary>
    public string? CertFileName { get; set; }

    /// <summary>
    /// 凭证密码，可为空
    /// </summary>
    public string? CertPassword { get; set; }
}
