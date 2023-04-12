namespace ThingsEdge.Contracts;

public static class IDeviceManagerExtensions
{
    public static string ToJsonText(this IDeviceManager deviceManager)
    {
        var channels = deviceManager.GetChannels();
        return JsonSerializer.Serialize(channels, new JsonSerializerOptions { WriteIndented = true });
    }
}
