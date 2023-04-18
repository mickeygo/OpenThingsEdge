namespace ThingsEdge.Providers.Ops.Configuration;

/// <summary>
/// 曲线数据配置。
/// </summary>
public sealed class CurveConfig
{
    /// <summary>
    /// 曲线文件本地存储目录。可以是完整路径，也可以是相对路径。
    /// </summary>
    public string? LocalDirectory { get; init; }

    /// <summary>
    /// 是否允许根据SN来打包数据，在 SN 存在的条件下为 true 时会 SN 建立文件夹，默认为 true。
    /// </summary>
    public bool AllowCategoryBySN { get; set; } = true;

    /// <summary>
    /// 是否要推送文件到远端服务器。
    /// </summary>
    public bool AllowCopy { get; init; }

    /// <summary>
    /// 曲线文件远端存储根目录（共享目录）。
    /// </summary>
    public string? RemoteDirectory { get; init; }

    /// <summary>
    /// 是否允许远程文件夹按日期分类，如 20230418
    /// </summary>
    public bool AllowRemoteCategoryByDay { get; set; }

    /// <summary>
    /// 是否允许远程文件夹按日期分类，如 202304
    /// </summary>
    public bool AllowRemoteCategoryByMonth { get; set; }
}
