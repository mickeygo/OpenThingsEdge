namespace ThingsEdge.Common.Utils;

/// <summary>
/// 文件夹帮助类
/// </summary>
public static class FolderUtil
{
    /// <summary>
    /// 创建文件夹，若已存在，则忽略。
    /// </summary>
    /// <param name="path">目录路径</param>
    public static void CreateIfNotExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    /// <summary>
    /// 获取目录已占用的大小，单位 byte。若目录不存在，则返回 0。
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <returns></returns>
    public static long GetSize(string path)
    {
        if (!Directory.Exists(path))
        {
            return 0;
        }

        // 枚举所有的文件，并累加文件长度。
        DirectoryInfo di = new(path);
        return GetSize(di);
    }

    /// <summary>
    /// 获取目录已占用的大小，单位 byte。若目录不存在，则返回 0。
    /// </summary>
    /// <param name="dirInfo">目录信息</param>
    /// <returns></returns>
    public static long GetSize(DirectoryInfo dirInfo)
    {
        if (!dirInfo.Exists)
        {
            return 0;
        }

        // 枚举所有的文件，并累加文件长度。
        return dirInfo.GetFiles("*", SearchOption.AllDirectories).Sum(s => s.Length);
    }

    /// <summary>
    /// 通过递归方式计算文件大小。
    /// </summary>
    /// <param name="dirInfo">目录信息</param>
    /// <returns></returns>
    public static long RecursionSize(DirectoryInfo dirInfo)
    {
        long size = 0;
        if (!dirInfo.Exists)
        {
            return size;
        }

        foreach (var fsi in dirInfo.EnumerateFileSystemInfos())
        {
            if (fsi is FileInfo fi)
            {
                size += fi.Length;
            }
            else if (fsi is DirectoryInfo di)
            {
                size += RecursionSize(di);
            }
        }

        return size;
    }

    /// <summary>
    /// 获取目录的树形结构信息，若没找到目录则返回 null。
    /// </summary>
    /// <param name="dirPath">目录路径</param>
    /// <returns></returns>
    public static FolderTreeInfo? GetFolderTreeInfo(string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            return default;
        }

        DirectoryInfo di = new(dirPath);
        var folderInfo = TreeFolderInfo(di);
        CalTreeFolderSizeInfo(folderInfo);
        return folderInfo;
    }

    private static void CalTreeFolderSizeInfo(FolderTreeInfo foldInfo)
    {
        if (foldInfo.FileSysType == FileSysType.Folder)
        {
            if (foldInfo.Children?.Any() ?? false)
            {
                foreach (var foldInfo2 in foldInfo.Children)
                {
                    CalTreeFolderSizeInfo(foldInfo2);
                }
            }

            foldInfo.Size = foldInfo.Children?.Sum(s => s.Size) ?? 0;
        }
    }

    private static FolderTreeInfo TreeFolderInfo(DirectoryInfo dirInfo)
    {
        FolderTreeInfo folderInfo = new()
        {
            Name = dirInfo.Name,
            FullName = dirInfo.FullName,
            Size = 0, // 文件夹延迟计算大小, 不采用 GetSize(dirInfo) 计算
            FileSysType = FileSysType.Folder,
            CreationTime = dirInfo.CreationTime,
            LastWriteTime = dirInfo.LastWriteTime,
        };

        foreach (var fsi in dirInfo.EnumerateFileSystemInfos())
        {
            folderInfo.Children ??= new();

            if (fsi is FileInfo fi)
            {
                folderInfo.Children.Add(new FolderTreeInfo
                {
                    Name = fi.Name,
                    FullName = fi.FullName,
                    Size = fi.Length,
                    FileSysType = FileSysType.File,
                    CreationTime = fi.CreationTime,
                    LastWriteTime = fi.LastWriteTime,
                });
            }
            else if (fsi is DirectoryInfo di)
            {
                folderInfo.Children.Add(TreeFolderInfo(di));
            }
        }

        return folderInfo;
    }

    /// <summary>
    /// 目录信息
    /// </summary>
    public sealed class FolderTreeInfo
    {
        /// <summary>
        /// 目录或文件的名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 目录或文件的完整名称
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// 目录或文件的大小
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public FileSysType FileSysType { get; set; }

        /// <summary>
        /// 目录或文件的创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 最近一次写入时间
        /// </summary>
        public DateTime LastWriteTime { get; set; }

        /// <summary>
        /// 子目录信息，若不存在，则为 null。
        /// </summary>
        public List<FolderTreeInfo>? Children { get; set; }
    }

    /// <summary>
    /// 文件类型
    /// </summary>
    public enum FileSysType
    {
        /// <summary>
        /// 目录
        /// </summary>
        Folder,

        /// <summary>
        /// 文件
        /// </summary>
        File,
    }
}
