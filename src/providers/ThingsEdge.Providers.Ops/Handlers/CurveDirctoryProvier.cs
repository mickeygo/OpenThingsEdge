using ThingsEdge.Providers.Ops.Configuration;

namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 曲线文件存储路径提供。
/// </summary>
internal sealed class CurveDirctoryProvier
{
    private static readonly string DefaultCurvesDirectory = Path.Combine(AppContext.BaseDirectory, "curves");

    private readonly OpsConfig _opsConfig;
    private bool _hasCreatedDir;

    public CurveDirctoryProvier(IOptionsMonitor<OpsConfig> opsConfig)
    {
        _opsConfig = opsConfig.CurrentValue;
    }

    /// <summary>
    /// 获取存储曲线的目录。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="IOException"></exception>
    /// <exception cref="PathTooLongException"></exception>
    public string GetCurveDirectory()
    {
        var curvesDir = _opsConfig.Curve?.Directory;
        if (string.IsNullOrWhiteSpace(curvesDir))
        {
            curvesDir = DefaultCurvesDirectory;
        }

        // 判断目录是否存在，没有就创建。
        if (!_hasCreatedDir)
        {
            if (!Directory.Exists(curvesDir))
            {
                Directory.CreateDirectory(curvesDir);
            }

            _hasCreatedDir = true;
        }

        // 考虑目录中文件存储的尺寸
        // 1）文件不删除
        // 2）文件按保存时间删除
        // 3）文件按保存数量删除
        // 4）按磁盘剩余空间来删除

        return curvesDir;
    }
}
