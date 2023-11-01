using ThingsEdge.Providers.Ops.Configuration;

namespace ThingsEdge.Providers.Ops.Handlers.Curve;

/// <summary>
/// 曲线存储器。
/// </summary>
internal sealed class CurveStorage : ISingletonDependency
{
    private static readonly string DefaultCurvesDirectory = Path.Combine(AppContext.BaseDirectory, "curves");

    private readonly CurveConfig _curveConfig;

    /// <summary>
    /// 曲线存储命名的分隔符。
    /// </summary>
    internal const char CurveNamedSeparator = '_';

    public CurveStorage(IOptionsMonitor<OpsConfig> opsConfig)
    {
        _curveConfig = opsConfig.CurrentValue.Curve;
    }

    /// <summary>
    /// 构建曲线的存储路径，文件命名格式: "[码]_[序号]_[时间]"。
    /// </summary>
    /// <param name="sn">曲线绑定的 SN</param>
    /// <param name="group">分组名称。</param>
    /// <param name="no">曲线所属的编号。</param>
    /// <param name="includeDatetime">是否包含时间, 默认为true</param>
    /// <returns></returns>
    /// <exception cref="IOException"></exception>
    /// <exception cref="PathTooLongException"></exception>
    public string BuildCurveFilePath(string? sn, string? group, string? no, bool includeDatetime = true)
    {
        var curvesDir = LocalCurvesDirectory();

        StringBuilder sb = new();

        if (!string.IsNullOrWhiteSpace(sn))
        {
            // SN 分组 ==> XXX/SN001/
            if (_curveConfig.AllowCategoryBySN)
            {
                curvesDir = Path.Combine(curvesDir, sn); // sn
            }

            // 文件名
            sb.Append(sn);
        }

        // SN 内部再分组 => XXX/[SN001]/[OP10]/
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
            sb.Append(no); // => SN001_2
        }

        // 当还没有设置文件名信息时，会设置日期为文件名称。
        if (sb.Length == 0 || includeDatetime)
        {
            if (sb.Length > 0)
            {
                sb.Append(CurveNamedSeparator);
            }
            sb.Append(DateTime.Now.ToString("yyyyMMddHHmmss")); // => SN001_2_yyyyMMddHHmmss
        }

        var filepath = Path.Combine(curvesDir, $"{sb}.{FileExt}"); // => SN001_2_yyyyMMddHHmmss.csv

        // TODO: 考虑目录中文件存储的大小，在存储空间不足时进行删除
        // 1）文件不删除
        // 2）文件按保存时间删除
        // 3）文件按保存数量删除
        // 4）按磁盘剩余空间来删除

        return filepath;
    }

    /// <summary>
    /// 尝试将文件拷贝到远程服务器（若设置了允许拷贝）。
    /// </summary>
    /// <param name="filepath">要推送的文件。</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<(bool ok, string? err)> TryCopyAsync(string filepath, CancellationToken cancellationToken)
    {
        TaskCompletionSource<(bool, string?)> tcs = new();

        if (cancellationToken.IsCancellationRequested)
        {
            tcs.TrySetCanceled();
            return tcs.Task;
        }

        if (!_curveConfig.AllowCopy)
        {
            tcs.TrySetResult((true, default));
            return tcs.Task;
        }

        var dir0 = _curveConfig.RemoteDirectory;
        if (string.IsNullOrWhiteSpace(dir0))
        {
            tcs.TrySetResult((true, default));
            return tcs.Task;
        }

        try
        {
            // 曲线以每个 SN 文件命名放置。
            var (ok, path2) = ExtractFilePathPostfix(filepath);
            if (!ok)
            {
                tcs.TrySetResult((true, default)); // 不能解析直接返回，不处理
                return tcs.Task;
            }

            var destFileName = Path.Combine(dir0, path2!);
            var dir2 = Path.GetDirectoryName(destFileName)!;
            if (!Directory.Exists(dir2))
            {
                Directory.CreateDirectory(dir2); // 创建目录时，相对路径会转换为绝对路径
            }

            // 可根据返回的文件路径做其他处理，如推送到远程服务器。
            File.Copy(filepath, destFileName);

            tcs.TrySetResult((true, default));
        }
        catch (Exception ex)
        {
            tcs.TrySetResult((false, ex.Message));
        }

        return tcs.Task;
    }

    private (bool ok, string? path) ExtractFilePathPostfix(string fileName)
    {
        var localDir = LocalCurvesDirectory();

        // 比较本地存储目录和文件路径，截取文件路径中不包含本地目录的文本。
        if (fileName.Length <= localDir.Length)
        {
            return (false, default);
        }

        var suffix = fileName[localDir.Length..]; // 含分隔符
        if (suffix != localDir)
        {
            return (false, default);
        }

        return (true, fileName[(localDir.Length + 1)..]); // 前缀不含分隔符
    }

    private string LocalCurvesDirectory()
    {
        var curvesDir = _curveConfig.LocalDirectory;
        if (string.IsNullOrWhiteSpace(curvesDir))
        {
            return DefaultCurvesDirectory;
        }

        return Path.GetFullPath(curvesDir);
    }

    private string FileExt =>
        _curveConfig.FileExt switch
        {
            CurveFileExt.JSON => "json",
            CurveFileExt.CSV => "csv",
            _ => throw new InvalidOperationException("曲线文件存储格式必须是 JSON 或 CSV"),
        };

}
