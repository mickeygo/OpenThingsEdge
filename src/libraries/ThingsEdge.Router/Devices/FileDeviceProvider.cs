namespace ThingsEdge.Router.Devices;

/// <summary>
/// 设备数据来源于本地文件。
/// </summary>
public sealed class FileDeviceProvider : IDeviceProvider
{
    private readonly string _configDirectory;
    private readonly string _tagsPath;

    public FileDeviceProvider()
    {
        _configDirectory = Path.Combine(AppContext.BaseDirectory, "config"); // "[执行目录]/config/"
        _tagsPath = Path.Combine(_configDirectory, "tags.conf");
    }

    /// <summary>
    /// 获取通道数据。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="JsonException"></exception>
    public List<Channel> GetChannels()
    {
        // 若有单文件，会从单文件中解析；若不存在，会从文件中解析。
        if (File.Exists(_tagsPath))
        {
            return GetChannelsFromSingleFile();
        }

        return GetChannelsFromFolder();
    }

    /// <summary>
    /// 从单一文件中解析
    /// </summary>
    /// <returns></returns>
    /// <exception cref="JsonException"></exception>
    private List<Channel> GetChannelsFromSingleFile()
    {
        var text = File.ReadAllText(Path.GetFullPath(_tagsPath));
        return JsonDeserialize<List<Channel>>(text) ?? [];
    }

    /// <summary>
    /// 从文件夹中解析
    /// </summary>
    /// <returns></returns>
    /// <exception cref="JsonException"></exception>
    private List<Channel> GetChannelsFromFolder()
    {
        // 目录结构:
        // 执行目录/config:
        //  w-- tags.conf
        //  d-- channels #通道
        //    d-- [Line01] #通道1
        //      w-- channel.conf #通道1配置
        //        d-- [S7-1200] #设备1
        //          w-- device.conf #设备1配置
        //          w-- OP010.conf #设备1下分组成员
        //          w-- OP020.conf
        //        d-- [S7-1500] #设备2
        //          w-- device.conf #设备2配置
        //          w-- OP030.conf
        //          w-- OP040.conf

        List<Channel> channels = [];

        var channelsDirPath = Path.Combine(_configDirectory, "channels");
        var channelsDirInfo = new DirectoryInfo(channelsDirPath);
        if (!channelsDirInfo.Exists)
        {
            return channels;
        }

        foreach (var channelDirInfo in channelsDirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly))
        {
            var channelConf = channelDirInfo.EnumerateFiles("channel.conf", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (channelConf is null)
            {
                continue;
            }

            // 解析 channelConf
            var text1 = ReadAllText(channelConf);
            var channel = JsonDeserialize<Channel>(text1);
            if (channel is null)
            {
                continue;
            }

            channels.Add(channel);

            foreach (var deviceDirInfo in channelDirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly))
            {
                var confs = deviceDirInfo.EnumerateFiles("*.conf", SearchOption.TopDirectoryOnly);

                var deviceConf = confs.FirstOrDefault(s => s.Name == "device.conf");
                if (deviceConf is null)
                {
                    continue;
                }

                // 解析 device.conf
                var text2 = ReadAllText(deviceConf);
                var device = JsonDeserialize<Device>(text2);
                if (device is null)
                {
                    continue;
                }

                channel.Devices.Add(device);

                foreach (var groupConf in confs.Where(s => s.Name != "device.conf"))
                {
                    // 解析 group.conf
                    var text3 = ReadAllText(groupConf);
                    var tagGroup = JsonDeserialize<TagGroup>(text3);
                    if (tagGroup is null)
                    {
                        continue;
                    }

                    device.TagGroups.Add(tagGroup);
                }
            }
        }

        return channels;
    }

    private static T? JsonDeserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip, // 允许注释
            AllowTrailingCommas = true, // 允许尾随逗号
            PropertyNameCaseInsensitive = true, // 属性名称匹配不区分大小写
        });
    }

    private static string ReadAllText(FileInfo fileInfo)
    {
        using var sr = fileInfo.OpenText();
        return sr.ReadToEnd();
    }
}
