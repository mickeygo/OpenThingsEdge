namespace ThingsEdge.Exchange.Configuration;

/// <summary>
/// 曲线配置选项
/// </summary>
public sealed class CurveOptions
{
    /// <summary>
    /// 曲线文件本地存储根目录。可以是完整路径，也可以是相对路径。
    /// </summary>
    public string? LocalRootDirectory { get; set; }

    /// <summary>
    /// 曲线存储文件格式，JSON / CSV，默认为 CSV 格式。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CurveFileExt FileType { get; set; } = CurveFileExt.CSV;

    /// <summary>
    /// 曲线存储文件名称的分隔符，默认下划线 "_"。
    /// </summary>
    public string CurveNamedSeparator { get; set; } = "_";

    /// <summary>
    /// 目录是否包含通道名称，默认为 true。
    /// </summary>
    public bool DirIncludeChannelName { get; set; } = true;

    /// <summary>
    /// 目录是否包含曲线名称，默认为 true。
    /// </summary>
    public bool DirIncludeCurveName { get; set; } = true;

    /// <summary>
    /// 文件路径中是否包含日期，格式为 "yyyyMMdd"，默认为 true。
    /// </summary>
    public bool DirIncludeDate { get; set; } = true;

    /// <summary>
    /// 文件路径中是否包含第一个主数据，若有需要可按配置顺序将关键信息放置第一位，默认为 true。
    /// </summary>
    public bool DirIncludeFirstMaster { get; set; } = true;

    /// <summary>
    /// 目录是否包含分组名称，默认为 true。
    /// </summary>
    public bool DirIncludeGroupName { get; set; } = true;

    /// <summary>
    /// 文件后缀是否包含日期，格式为 "yyyyMMddHHmmss"。
    /// </summary>
    public bool SuffixIncludeDatetime { get; set; } = true;

    /// <summary>
    /// 文件中允许写入最大的次数，默认为 4096。
    /// </summary>
    public int AllowMaxWriteCount { get; set; } = 4096;

    /// <summary>
    /// 曲线数据保存时要移除的尾部条数，0 表示不移除，默认为 0。
    /// </summary>
    /// <remarks>
    /// 若信号点扫描频率与曲线数据扫描频率相差较大时，在发起关闭信号时可能实际已经多采集了
    /// </remarks>
    public int RemoveTailCountBeforeSaving { get; set; }

    /// <summary>
    /// 是否返回曲线保存文件的相对路径，false 表示返回的文件绝对路径，默认为 true。
    /// </summary>
    public bool ReturnRelativeFilePath { get; set; } = true;

    /// <summary>
    /// 是否要推送文件到远端服务器。
    /// </summary>
    public bool AllowCopy { get; set; }

    /// <summary>
    /// 曲线文件远端存储根目录（共享目录）。
    /// </summary>
    public string? RemoteRootDirectory { get; set; }

    /// <summary>
    /// 本地文件保存最大天数，会删除最近访问时间超过指定天数的文件和文件夹，0 表示不删除，默认 0。
    /// </summary>
    public int RetainedDayLimit { get; set; }
}
