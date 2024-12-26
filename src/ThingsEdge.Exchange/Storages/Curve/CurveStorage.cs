using ThingsEdge.Exchange.Configuration;
using ThingsEdge.Exchange.Contracts;
using ThingsEdge.Exchange.Utils;

namespace ThingsEdge.Exchange.Storages.Curve;

/// <summary>
/// 曲线存储器
/// </summary>
internal sealed class CurveStorage(CurveContainer curveContainer, IOptions<ExchangeOptions> options)
{
    private readonly Lazy<RollingFile> _rollingFile = new(CreateRollingFile(options.Value.Curve.LocalRootDirectory, options.Value.Curve.RetainedDayLimit));

    /// <summary>
    /// 如果容器中不存在指定的写入器，则创建。
    /// </summary>
    /// <param name="tagId">标记唯一 Id</param>
    /// <param name="model">曲线模型。</param>
    /// <exception cref="IOException"></exception>
    /// <exception cref="PathTooLongException"></exception>
    public (bool ok, string? err) CreateIfNotExists(string tagId, CurveModel model)
    {
        try
        {
            curveContainer.GetOrCreate(tagId, model, options.Value.Curve.FileType, BuildFilePath);
            return (true, default);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// 检测是否可以写入 Body 数据。
    /// </summary>
    /// <param name="tagId">标记 Id</param>
    /// <returns></returns>
    public (bool ok, string? err) CanWriteBody(string tagId)
    {
        if (!curveContainer.TryGet(tagId, out var state))
        {
            return (false, default); // 此处不返回错误消息
        }

        if (state.Writer.IsClosed)
        {
            return (false, "曲线文件写入器已经关闭");
        }

        if (state.Writer.WrittenCount > options.Value.Curve.AllowMaxWriteCount)
        {
            return (false, $"曲线文件写入次数已达到设置上限 {options.Value.Curve.AllowMaxWriteCount}");
        }

        return (true, default);
    }

    /// <summary>
    /// 写入头信息。
    /// </summary>
    /// <param name="tagId"></param>
    /// <param name="header"></param>
    /// <returns></returns>
    public (bool ok, string? err) WriteHeader(string tagId, IEnumerable<string> header)
    {
        if (!curveContainer.TryGet(tagId, out var state))
        {
            return (false, "未找到数据写入器");
        }

        state.Writer.WriteHeader(header);
        return (true, default);
    }

    /// <summary>
    /// 写入一行数据。
    /// </summary>
    /// <param name="tagId"></param>
    /// <param name="payloads"></param>
    public (bool ok, string? err) WriteLine(string tagId, IEnumerable<PayloadData> payloads)
    {
        if (!curveContainer.TryGet(tagId, out var state))
        {
            return (false, "未找到数据写入器");
        }

        if (state.Writer.WrittenCount > options.Value.Curve.AllowMaxWriteCount)
        {
            return (false, $"写入数据到达了允许的上限值 {options.Value.Curve.AllowMaxWriteCount}");
        }

        state.Writer.WriteLineBody(payloads);
        return (true, default);
    }

    /// <summary>
    /// 保存文件，并拷贝文件到远端（若配置）
    /// </summary>
    /// <param name="tagId">标记 Id</param>
    public async Task<(bool ok, CurveModel? model, string? path)> SaveAsync(string tagId)
    {
        
        var (ok, state) = await curveContainer.SaveAndRemoveAsync(tagId, options.Value.Curve.RemoveTailCountBeforeSaving).ConfigureAwait(false);
        if (ok)
        {
            if (options.Value.Curve.AllowCopy && !string.IsNullOrWhiteSpace(options.Value.Curve.RemoteRootDirectory))
            {
                CopyTo(state!.Writer.FilePath, state!.Writer.RelativePath, options.Value.Curve.RemoteRootDirectory);
            }

            // 在设定目录最大容量后，超出容量将进行删除
            if (options.Value.Curve.RetainedDayLimit > 0)
            {
                _rollingFile.Value.Increment();
            }

            var path = options.Value.Curve.ReturnRelativeFilePath ? state!.Writer.RelativePath : state!.Writer.FilePath;
            return (true, state!.Model, path);
        }

        return (false, default, default);
    }

    /// <summary>
    /// 构建文件存储绝对路径
    /// </summary>
    private (string path, string relativePath) BuildFilePath(CurveModel model)
    {
        var rootDir = LocalRootDirectory();
        var (relativeDir, fileName) = BuildRelativeFilePath(model);
        var dir = Path.Combine(rootDir, relativeDir);
        DirectoryUtils.CreateIfNotExists(dir);

        return (Path.Combine(dir, fileName), Path.Combine(relativeDir, fileName));
    }

    /// <summary>
    /// 构建文件存储相对路径
    /// </summary>
    private (string relativeDir, string fileName) BuildRelativeFilePath(CurveModel model)
    {
        var now = DateTime.Now;
        var curveDir = "";

        // 文件包含通道名称
        if (options.Value.Curve.DirIncludeChannelName)
        {
            curveDir = Path.Combine(curveDir, model.ChannelName); // root/L1/
        }

        // 文件包含曲线名称
        if (options.Value.Curve.DirIncludeCurveName && !string.IsNullOrWhiteSpace(model.CurveName))
        {
            curveDir = Path.Combine(curveDir, model.CurveName); // root/[L1]/Welding
        }

        // 路径包含日期
        if (options.Value.Curve.DirIncludeDate)
        {
            curveDir = Path.Combine(curveDir, now.ToString("yyyyMMdd")); // root/[L1]/[Welding]/20230101
        }

        // 按 SN 打包
        if (options.Value.Curve.DirIncludeFirstMaster && model.Masters.Count > 0)
        {
            curveDir = Path.Combine(curveDir, model.Masters[0].GetString()); // root/[L1]/[Welding]/[20230101]/SN001/
        }

        // SN 内部再分组
        if (options.Value.Curve.DirIncludeGroupName && !string.IsNullOrWhiteSpace(model.GroupName))
        {
            curveDir = Path.Combine(curveDir, model.GroupName); // root/[L1]/[Welding]/[20230101]/[SN001]/OP10/
        }

        var masters2 = model.Masters.Select(s => s.GetString()).ToList();
        // 当还没有设置文件名称时也会使用日期作为文件名称。
        if (masters2.Count == 0 || options.Value.Curve.SuffixIncludeDatetime)
        {
            masters2.Add(now.ToString("yyyyMMddHHmmss"));
        }
        var fileName = $"{string.Join(options.Value.Curve.CurveNamedSeparator, masters2)}.{FileExt}";

        // 文件最终名称：root/[L1]/[Welding]/[20230101]/[SN001]/[OP010]/SN001_2_yyyyMMddHHmmss.csv
        return (curveDir, fileName);
    }

    /// <summary>
    /// 拷贝到远端目录
    /// </summary>
    /// <param name="filePath">文件绝对路径</param>
    /// <param name="relativeFilePath">文件相对路径</param>
    /// <param name="remoteDir">远端目录</param>
    /// <returns></returns>
    private static (bool ok, string? err) CopyTo(string filePath, string relativeFilePath, string remoteDir)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return (true, default);
            }

            var destFileName = Path.Combine(remoteDir, relativeFilePath);
            var dir2 = Path.GetDirectoryName(destFileName);
            DirectoryUtils.CreateIfNotExists(dir2!); // 创建目录时，相对路径会转换为绝对路径

            // 可根据返回的文件路径做其他处理，如推送到远程服务器。
            File.Copy(filePath, destFileName);
        }
        catch
        {
        }

        return (true, default);
    }

    /// <summary>
    /// 本地曲线根目录
    /// </summary>
    /// <returns></returns>
    private string LocalRootDirectory()
    {
        return LocalRootDirectory(options.Value.Curve.LocalRootDirectory);
    }

    private string FileExt =>
       options.Value.Curve.FileType switch
       {
           CurveFileExt.JSON => "json",
           CurveFileExt.CSV => "csv",
           _ => throw new InvalidOperationException("曲线文件存储格式必须是 JSON 或 CSV"),
       };

    private static RollingFile CreateRollingFile(string? localRootDirectory, int retainedSizeLimit)
    {
        var rootDir = LocalRootDirectory(localRootDirectory);
        return new RollingFile(rootDir, retainedSizeLimit);
    }

    private static string LocalRootDirectory(string? localRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(localRootDirectory))
        {
            return Path.Combine(AppContext.BaseDirectory, "curves");
        }

        return Path.GetFullPath(localRootDirectory);
    }
}
