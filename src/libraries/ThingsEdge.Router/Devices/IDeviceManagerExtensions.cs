namespace ThingsEdge.Router.Devices;

public static class IDeviceManagerExtensions
{
    /// <summary>
    /// 重新加载数据，会清空缓存重新加载数据。
    /// </summary>
    /// <param name="deviceManager"></param>
    /// <returns></returns>
    public static List<Device> ReloadDevices(this IDeviceManager deviceManager)
    {
        deviceManager.Refresh();
        return deviceManager.GetDevices();
    }

    /// <summary>
    /// 获取所有的设备。
    /// </summary>
    /// <param name="deviceManager"></param>
    /// <returns></returns>
    public static List<Device> GetDevices(this IDeviceManager deviceManager)
    {
        var channels = deviceManager.GetChannels();
        return channels.SelectMany(s => s.Devices).ToList();
    }

    /// <summary>
    /// 获取指定通道下指定名称的设备。
    /// </summary>
    /// <param name="deviceManager"></param>
    /// <param name="channelName">通道名称</param>
    /// <param name="deviceName">设备名称</param>
    /// <returns></returns>
    public static Device? GetDevice(this IDeviceManager deviceManager, string channelName, string deviceName)
    {
        var channels = deviceManager.GetChannels();
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
    /// <param name="deviceManager"></param>
    /// <param name="deviceId">设备Id。</param>
    /// <returns></returns>
    public static Device? GetDevice(this IDeviceManager deviceManager, string deviceId)
    {
        var devices = deviceManager.GetDevices();
        return devices.FirstOrDefault(s => s.DeviceId == deviceId);
    }

    /// <summary>
    /// 获取指定的设备。
    /// </summary>
    /// <param name="deviceManager"></param>
    /// <param name="deviceId">设备Id。</param>
    /// <returns></returns>
    public static (string? channelName, Device? device) GetDevice2(this IDeviceManager deviceManager, string deviceId)
    {
        var channels = deviceManager.GetChannels();
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
    /// <param name="deviceManager"></param>
    /// <returns></returns>
    public static string ToJsonText(this IDeviceManager deviceManager)
    {
        var channels = deviceManager.GetChannels();
        return JsonSerializer.Serialize(channels, new JsonSerializerOptions { WriteIndented = true });
    }
}
