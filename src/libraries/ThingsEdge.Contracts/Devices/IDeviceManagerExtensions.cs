namespace ThingsEdge.Contracts.Devices;

public static class IDeviceManagerExtensions
{
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
