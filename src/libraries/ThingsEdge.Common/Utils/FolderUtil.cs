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
    /// 获取目录已占用的大小，单位 byte。
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <returns></returns>
    public static long GetSize(string path)
    {
        if (!Directory.Exists(path))
        {
            return 0;
        }

        DirectoryInfo di = new(path);
        return di.GetFiles("*", SearchOption.AllDirectories).Sum(s => s.Length);
    }
}
