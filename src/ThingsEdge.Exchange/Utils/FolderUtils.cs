namespace ThingsEdge.Exchange.Utils;

/// <summary>
/// 文件夹帮助类
/// </summary>
public static class FolderUtils
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
}
