using ThingsEdge.Providers.Ops.Configuration;

namespace ThingsEdge.Providers.Ops.Handlers.Curve;

/// <summary>
/// 曲线存储器
/// </summary>
internal sealed class CurveStorage : ISingletonDependency
{
    private static readonly string DefaultCurveRootDirectory = Path.Combine(AppContext.BaseDirectory, "curves");

    private readonly Lazy<RollingFile> _rollingFile;

    private readonly CurveContainer _curveContainer;
    private readonly CurveConfig _curveConfig;

    public CurveStorage(CurveContainer curveContainer, IOptionsMonitor<OpsConfig> opsConfig)
    {
        _curveContainer = curveContainer;
        _curveConfig = opsConfig.CurrentValue.Curve;

        _rollingFile = new(CreateRollingFile(_curveConfig.LocalRootDirectory, _curveConfig.RetainedDayLimit));
    }

    /// <summary>
    /// 尝试获取文件写入器。
    /// </summary>
    /// <param name="tagId">对象唯一值。</param>
    /// <param name="state"></param>
    /// <returns></returns>
    public bool TryGetWriter(string tagId, [MaybeNullWhen(false)] out CurveContainerState state)
    {
        state = default;
        if (_curveContainer.TryGet(tagId, out var state2))
        {
            state = state2;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 构建曲线的写入器。
    /// </summary>
    /// <param name="tagId">标记唯一 Id</param>
    /// <param name="model">曲线模型。</param>
    /// <returns></returns>
    /// <exception cref="IOException"></exception>
    /// <exception cref="PathTooLongException"></exception>
    public ICurveWriter GetOrCreate(string tagId, CurveModel model)
    {
        var curveFilePath = BuildCurveFilePath(model);
        return _curveContainer.GetOrCreate(tagId, curveFilePath, model, _curveConfig.FileType);
    }

    /// <summary>
    /// 检测是否可以写入 Body 数据。
    /// </summary>
    /// <param name="tagId">标记 Id</param>
    /// <returns></returns>
    public (bool ok, string? err, ICurveWriter? writer) CanWriteBody(string tagId)
    {
        if (!_curveContainer.TryGet(tagId, out var state))
        {
            return (false, default, default); // 此处不返回错误消息
        }

        if (state.Writer.IsClosed)
        {
            return (false, "曲线文件写入器已经关闭", default);
        }

        if (state.Writer.WrittenCount > _curveConfig.AllowMaxWriteCount)
        {
            return (false, $"曲线文件写入次数已达到设置上限 {_curveConfig.AllowMaxWriteCount}", default);
        }

        return (true, default, state.Writer);
    }

    /// <summary>
    /// 保存文件，并拷贝文件到远端（若配置）
    /// </summary>
    /// <param name="tagId">标记 Id</param>
    public (bool ok, CurveModel? model, string? path) Save(string tagId)
    {
        if (_curveContainer.TrySaveAndRemove(tagId, out var state))
        {
            if (_curveConfig.AllowCopy && !string.IsNullOrWhiteSpace(_curveConfig.RemoteRootDirectory))
            {
                CopyTo(state.Writer.FilePath, _curveConfig.RemoteRootDirectory);
            }

            // 在设定目录最大容量后，超出容量将进行删除
            if (_curveConfig.RetainedDayLimit > 0)
            {
                _rollingFile.Value.Increment();
            }

            return (true, state.Model, state.Writer.FilePath);
        }

        return (false, default, default);
    }

    private string BuildCurveFilePath(CurveModel model)
    {
        var now = DateTime.Now;

        var curveRootDir = LocalCurveRootDirectory();
        var curveDir = curveRootDir; // root/

        // 文件包含通道名称
        if (_curveConfig.DirIncludeChannelName)
        {
            curveDir = Path.Combine(curveDir, model.ChannelName); // root/L1/
        }

        // 文件包含曲线名称
        if (_curveConfig.DirIncludeCurveName && !string.IsNullOrWhiteSpace(model.CurveName))
        {
            curveDir = Path.Combine(curveDir, model.CurveName); // root/[L1]/拧紧
        }

        // 路径包含日期
        if (_curveConfig.DirIncludeDate)
        {
            curveDir = Path.Combine(curveDir, now.ToString("yyyyMMdd")); // root/[L1]/[拧紧]/20230101
        }

        // 按 SN 打包
        if (_curveConfig.DirIncludeSN && !string.IsNullOrWhiteSpace(model.SN))
        {
            curveDir = Path.Combine(curveDir, model.SN); // root/[L1]/[拧紧]/[20230101]/SN001/
        }

        // SN 内部再分组
        if (_curveConfig.DirIncludeGroupName && !string.IsNullOrWhiteSpace(model.GroupName))
        {
            curveDir = Path.Combine(curveDir, model.GroupName); // root/[L1]/[拧紧]/[20230101]/[SN001]/OP10/
        }

        // 创建对应的文件夹。
        FolderUtil.CreateIfNotExists(curveDir);

        StringBuilder sbFilename = new(model.SN); // 文件名称
        if (!string.IsNullOrWhiteSpace(model.No))
        {
            if (sbFilename.Length > 0)
            {
                sbFilename.Append(_curveConfig.CurveNamedSeparator);
            }
            sbFilename.Append(model.No); // => SN001_2
        }

        // 当还没有设置文件名信息时，会设置日期为文件名称。
        if (sbFilename.Length == 0 || _curveConfig.SuffixIncludeDatetime)
        {
            if (sbFilename.Length > 0)
            {
                sbFilename.Append(_curveConfig.CurveNamedSeparator);
            }
            sbFilename.Append(now.ToString("yyyyMMddHHmmss")); // => SN001_2_yyyyMMddHHmmss
        }

        var filepath = Path.Combine(curveDir, $"{sbFilename}.{FileExt}"); // => SN001_2_yyyyMMddHHmmss.csv

        return filepath;
    }

    /// <summary>
    /// 拷贝到远端目录
    /// </summary>
    /// <param name="filepath">文件路径</param>
    /// <param name="remoteDir">远端目录</param>
    /// <returns></returns>
    private (bool ok, string? err) CopyTo(string filepath, string remoteDir)
    {
        try
        {
            // 曲线以每个 SN 文件命名放置。
            var (ok, path2) = ExtractFilePathPostfix(filepath);
            if (!ok)
            {
                return (true, default); // 不能解析直接返回，不处理
            }

            var destFileName = Path.Combine(remoteDir, path2!);
            var dir2 = Path.GetDirectoryName(destFileName)!;
            FolderUtil.CreateIfNotExists(dir2); // 创建目录时，相对路径会转换为绝对路径

            // 可根据返回的文件路径做其他处理，如推送到远程服务器。
            File.Copy(filepath, destFileName);
        }
        catch
        {
        }

        return (true, default);
    }

    /// <summary>
    /// 文件后缀
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private (bool ok, string? path) ExtractFilePathPostfix(string fileName)
    {
        var localRootDir = LocalCurveRootDirectory();

        // 比较本地存储目录和文件路径，截取文件路径中不包含本地目录的文本。
        if (fileName.Length <= localRootDir.Length)
        {
            return (false, default);
        }

        var suffix = fileName[localRootDir.Length..]; // 含分隔符
        if (suffix != localRootDir)
        {
            return (false, default);
        }

        return (true, fileName[(localRootDir.Length + 1)..]); // 前缀不含分隔符
    }

    /// <summary>
    /// 本地曲线根目录
    /// </summary>
    /// <returns></returns>
    private string LocalCurveRootDirectory()
    {
        return LocalCurveRootDirectory(_curveConfig.LocalRootDirectory);
    }

    private string FileExt =>
       _curveConfig.FileType switch
       {
           CurveFileExt.JSON => "json",
           CurveFileExt.CSV => "csv",
           _ => throw new InvalidOperationException("曲线文件存储格式必须是 JSON 或 CSV"),
       };

    private static RollingFile CreateRollingFile(string? localRootDirectory, long retainedSizeLimit)
    {
        var rootDir = LocalCurveRootDirectory(localRootDirectory);
        return new RollingFile(rootDir, retainedSizeLimit);
    }

    private static string LocalCurveRootDirectory(string? localRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(localRootDirectory))
        {
            return DefaultCurveRootDirectory;
        }

        return Path.GetFullPath(localRootDirectory);
    }
}
