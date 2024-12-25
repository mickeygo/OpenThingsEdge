namespace ThingsEdge.Exchange.Storages.Curve;

/// <summary>
/// 文件滚动更新
/// </summary>
/// <param name="rootPath">文件根目录</param>
/// <param name="retainedDayLimit">要保留的天数限制</param>
internal sealed class RollingFile(string rootPath, long retainedDayLimit)
{
    private DateTime? _firstWriteTime;

    private static readonly object s_lock = new();

    /// <summary>
    /// 文件整件
    /// </summary>
    public void Increment()
    {
        if (retainedDayLimit == 0)
        {
            return;
        }

        var today = DateTime.Today;
        if (_firstWriteTime.HasValue && (today - _firstWriteTime.Value).Days <= retainedDayLimit)
        {
            return;
        }

        lock (s_lock)
        {
            try
            {
                DirectoryInfo dirInfo = new(rootPath);
                foreach (var fsi in dirInfo.EnumerateFileSystemInfos("*", SearchOption.AllDirectories))
                {
                    var lastWriteDate = fsi.LastWriteTime.Date;

                    // 目前只删除文件（目录中有文件异动，不会修改父目录的时间信息）
                    if (fsi is FileInfo fi)
                    {
                        if ((today - lastWriteDate).Days > retainedDayLimit)
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
