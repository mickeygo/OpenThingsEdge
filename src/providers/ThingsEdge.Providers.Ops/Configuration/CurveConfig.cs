﻿namespace ThingsEdge.Providers.Ops.Configuration;

/// <summary>
/// 曲线数据配置。
/// </summary>
public sealed class CurveConfig
{
    /// <summary>
    /// 曲线文件本地存储根目录。可以是完整路径，也可以是相对路径。
    /// </summary>
    public string? LocalRootDirectory { get; init; }

    /// <summary>
    /// 曲线存储文件格式，JSON / CSV，默认为 CSV 格式。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CurveFileExt FileType { get; init; } = CurveFileExt.CSV;

    /// <summary>
    /// 曲线存储文件名称的分隔符，默认 "_"。
    /// </summary>
    public string CurveNamedSeparator { get; init; } = "_";

    /// <summary>
    /// 目录是否包含通道名称，默认为 true。
    /// </summary>
    public bool DirIncludeChannelName { get; init; } = true;

    /// <summary>
    /// 目录是否包含曲线名称，默认为 true。
    /// </summary>
    public bool DirIncludeCurveName { get; init; } = true;

    /// <summary>
    /// 文件路径是否包含日期，格式为 "yyyyMMdd"，默认为 true。
    /// </summary>
    public bool DirIncludeDate { get; init; } = true;

    /// <summary>
    /// 是否允许根据SN来打包数据，在 SN 存在的条件下为 true 时会 SN 建立文件夹，默认为 true。
    /// </summary>
    public bool AllowCategoryBySN { get; init; } = true;

    /// <summary>
    /// 目录是否包含分组名称，默认为 true。
    /// </summary>
    public bool DirIncludeGroupName { get; init; } = true;

    /// <summary>
    /// 文件后缀是否包含日期，格式为 "yyyyMMddHHmmss"。
    /// </summary>
    public bool SuffixIncludeDatetime { get; init; }

    /// <summary>
    /// 文件中允许写入最大的次数，默认为 <see cref="short.MaxValue"/>。
    /// </summary>
    public int AllowMaxWriteCount { get; init; } = short.MaxValue;

    /// <summary>
    /// 是否要推送文件到远端服务器。
    /// </summary>
    public bool AllowCopy { get; init; }

    /// <summary>
    /// 曲线文件远端存储根目录（共享目录）。
    /// </summary>
    public string? RemoteRootDirectory { get; init; }

    /// <summary>
    /// 本地文件占用的最大空间，超过后会删除最原始的文件，0 表示不删除（单位 M）。
    /// </summary>
    public int RetainedSizeLimit { get; init; }
}
