namespace ThingsEdge.Router.Devices;

public static class IDeviceManagerExtensions
{
    /// <summary>
    /// 重新加载数据，会清空缓存重新加载数据。
    /// </summary>
    /// <param name="deviceFactory"></param>
    /// <returns></returns>
    public static List<Device> ReloadDevices(this IDeviceFactory deviceFactory)
    {
        deviceFactory.Refresh();
        return deviceFactory.GetDevices();
    }

    /// <summary>
    /// 获取所有的设备。
    /// </summary>
    /// <param name="deviceFactory"></param>
    /// <returns></returns>
    public static List<Device> GetDevices(this IDeviceFactory deviceFactory)
    {
        var channels = deviceFactory.GetChannels();
        return channels.SelectMany(s => s.Devices).ToList();
    }

    /// <summary>
    /// 获取指定通道下指定名称的设备。
    /// </summary>
    /// <param name="deviceFactory"></param>
    /// <param name="channelName">通道名称</param>
    /// <param name="deviceName">设备名称</param>
    /// <returns></returns>
    public static Device? GetDevice(this IDeviceFactory deviceFactory, string channelName, string deviceName)
    {
        var channels = deviceFactory.GetChannels();
        var channel = channels.FirstOrDefault(s => s.Name == channelName);
        if (channel == null)
        {
            return default;
        }

        return channel.Devices.FirstOrDefault(s => s.Name == deviceName);
    }

    /// <summary>
    /// 获取指定的设备。
    /// </summary>
    /// <param name="deviceFactory"></param>
    /// <param name="deviceId">设备Id。</param>
    /// <returns></returns>
    public static Device? GetDevice(this IDeviceFactory deviceFactory, string deviceId)
    {
        var devices = deviceFactory.GetDevices();
        return devices.FirstOrDefault(s => s.DeviceId == deviceId);
    }

    /// <summary>
    /// 获取指定的设备。
    /// </summary>
    /// <param name="deviceFactory"></param>
    /// <param name="deviceId">设备Id。</param>
    /// <returns></returns>
    public static (string? channelName, Device? device) GetDevice2(this IDeviceFactory deviceFactory, string deviceId)
    {
        var channels = deviceFactory.GetChannels();
        foreach (var channel in channels)
        {
            var device = channel.Devices.FirstOrDefault(s => s.DeviceId == deviceId);
            if (device != null)
            {
                return (channel.Name, device);
            }
        }

        return (default, default);
    }

    /// <summary>
    /// 将 Channel 集合以 Json 输出。
    /// </summary>
    /// <param name="deviceFactory"></param>
    /// <returns></returns>
    public static string ToJsonText(this IDeviceFactory deviceFactory)
    {
        var channels = deviceFactory.GetChannels();
        return JsonSerializer.Serialize(channels, new JsonSerializerOptions { WriteIndented = true });
    }
}
