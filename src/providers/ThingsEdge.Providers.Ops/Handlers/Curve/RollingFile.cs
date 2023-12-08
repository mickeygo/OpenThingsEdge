namespace ThingsEdge.Providers.Ops.Handlers.Curve;

/// <summary>
/// 文件滚动更新
/// </summary>
internal sealed class RollingFile
{
    private readonly string _rootPath;
    private readonly long _retainedDayLimit;
    private DateTime? _firstWriteTime;

    private static readonly object _lock = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rootPath">文件根目录</param>
    /// <param name="retainedDayLimit">要保留的天数限制</param>
    public RollingFile(string rootPath, long retainedDayLimit)
    {
        _rootPath = rootPath;
        _retainedDayLimit = retainedDayLimit;
    }

    /// <summary>
    /// 文件整件
    /// </summary>
    public void Increment()
    {
        if (_retainedDayLimit == 0)
        {
            return;
        }

        var today = DateTime.Today;
        if (_firstWriteTime.HasValue && (today - _firstWriteTime.Value).Days <= _retainedDayLimit)
        {
            return;
        }

        lock (_lock)
        {
            try
            {
                DirectoryInfo dirInfo = new(_rootPath);
                foreach (var fsi in dirInfo.EnumerateFileSystemInfos("*", SearchOption.AllDirectories))
                {
                    var lastWriteDate = fsi.LastWriteTime.Date;

                    // 目前只删除文件（目录中有文件异动，不会修改父目录的时间信息）
                    if (fsi is FileInfo fi)
                    {
                        if ((today - lastWriteDate).Days > _retainedDayLimit)
                        {
                            fi.Delete();
                        }
                        else if (_firstWriteTime is null || _firstWriteTime > lastWriteDate)
                        {
                            // 现有文件中，找到最先前的文件，更新写入时间
                            _firstWriteTime = lastWriteDate;
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }
}
