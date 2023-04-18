﻿using ThingsEdge.Providers.Ops.Configuration;

namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 曲线存储器。
/// </summary>
internal sealed class CurveStorage
{
    private static readonly string DefaultCurvesDirectory = Path.Combine(AppContext.BaseDirectory, "curves");

    private readonly OpsConfig _opsConfig;

    /// <summary>
    /// 曲线存储命名的分隔符。
    /// </summary>
    internal const char CurveNamedSeparator = '_';

    public CurveStorage(IOptionsMonitor<OpsConfig> opsConfig)
    {
        _opsConfig = opsConfig.CurrentValue;
    }

    /// <summary>
    /// 构建曲线的存储路径，文件命名格式: "[码]_[序号]_[时间]"。
    /// </summary>
    /// <param name="group">分组名称。</param>
    /// <param name="sn">曲线绑定的 SN</param>
    /// <param name="no">曲线所属的编号。</param>
    /// <param name="includeDatetime">是否包含时间, 默认为true</param>
    /// <returns></returns>
    /// <exception cref="IOException"></exception>
    /// <exception cref="PathTooLongException"></exception>
    public string BuildCurveFilePath(string? group, string? sn, string? no, bool includeDatetime = true)
    {
        var curvesDir = _opsConfig.Curve.LocalDirectory;
        if (string.IsNullOrWhiteSpace(curvesDir))
        {
            curvesDir = DefaultCurvesDirectory;
        }

        StringBuilder sb = new();
       
        if (!string.IsNullOrWhiteSpace(sn))
        {
            // SN 分组 ==> XXX/SN001/
            if (_opsConfig.Curve.AllowCategoryBySN)
            {
                curvesDir = Path.Combine(curvesDir, sn); // sn
            }

            // 文件名
            sb.Append(sn);
        }

        // SN 内部再分组 ==> XXX/[SN001]/OP10/
        if (!string.IsNullOrWhiteSpace(group))
        {
            curvesDir = Path.Combine(curvesDir, group);
        }

        // 创建对应的文件夹。
        if (!Directory.Exists(curvesDir))
        {
            Directory.CreateDirectory(curvesDir);
        }

        if (!string.IsNullOrWhiteSpace(no))
        {
            if (sb.Length > 0)
            {
                sb.Append(CurveNamedSeparator);
            }
            sb.Append(no); // [sn_]no
        }

        if (sb.Length == 0 || includeDatetime)
        {
            if (sb.Length > 0)
            {
                sb.Append(CurveNamedSeparator);
            }
            sb.Append(DateTime.Now.ToString("yyyyMMddHHmmss")); // [sn_][no_]yyyyMMddHHmmss
        }

        var filepath = Path.Combine(curvesDir, sb.ToString(), ".csv"); // [sn_][no_]yyyyMMddHHmmss.csv

        // 考虑目录中文件存储的尺寸
        // 1）文件不删除
        // 2）文件按保存时间删除
        // 3）文件按保存数量删除
        // 4）按磁盘剩余空间来删除

        return filepath;
    }

    /// <summary>
    /// 推送文件。
    /// </summary>
    /// <param name="filepath">要推送的文件。</param>
    /// <returns></returns>
    public Task<(bool ok, string? err)> TryCopyAsync(string filepath, CancellationToken cancellationToken)
    {
        TaskCompletionSource<(bool, string?)> tcs = new();

        if (cancellationToken.IsCancellationRequested)
        {
            tcs.TrySetCanceled();
            return tcs.Task;
        }

        if (!_opsConfig.Curve.AllowCopy)
        {
            tcs.TrySetResult((true, default));
            return tcs.Task;
        }

        var dir0 = _opsConfig.Curve.RemoteDirectory;
        if (string.IsNullOrWhiteSpace(dir0))
        {
            tcs.TrySetResult((true, default));
            return tcs.Task;
        }

        try
        {
            // 曲线以每个 SN 文件命名放置。
            var fileName = Path.GetFileNameWithoutExtension(filepath) ?? "";
            var sn = SplitName(fileName) ?? fileName;
            var dir2 = Path.Combine(dir0, sn);
            if (!Directory.Exists(dir2))
            {
                Directory.CreateDirectory(dir2);
            }

            // 可根据返回的文件路径做其他处理，如推送到远程服务器。
            var destFileName = Path.Combine(dir2, Path.GetFileName(filepath));
            File.Copy(filepath, destFileName);

            tcs.TrySetResult((true, default));
        }
        catch (Exception ex)
        {
            tcs.TrySetResult((false, ex.Message));
        }

        return tcs.Task;
    }

    private static string? SplitName(string fileName)
    {
        var index = fileName.IndexOf(DefaultCurvesDirectory);
        if (index <= 0)
        {
            return default;
        }

        return fileName[..index];
    }
}