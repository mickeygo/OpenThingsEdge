namespace ThingsEdge.Router.Devices;

/// <summary>
/// 设备数据来源于本地文件。
/// </summary>
public sealed class FileDeviceProvider : IDeviceProvider
{
    /// <summary>
    /// 置件文配路径，默认为 "[执行目录]/config/tags.conf"，可以为相对路径。
    /// </summary>
    public string? FilePath { get; set; } = Path.Combine(AppContext.BaseDirectory, "config", "tags.conf");

    /// <summary>
    /// 获取通道数据。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    public List<Channel> GetChannels()
    {
        if (string.IsNullOrEmpty(FilePath))
        {
            return new(0);
        }

        var context = File.ReadAllText(Path.GetFullPath(FilePath));
        return JsonSerializer.Deserialize<List<Channel>>(context) ?? new(0);
    }
}
