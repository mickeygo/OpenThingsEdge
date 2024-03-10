namespace ThingsEdge.Router.Devices;

internal sealed class DefaultDeviceFactory : IDeviceFactory
{
    public const string CacheName = "__ThingsEdge.Device.Cache";

    private readonly IDeviceProvider _deviceSource;
    private readonly IMemoryCache _cache;

    public DefaultDeviceFactory(IDeviceProvider deviceSource, IMemoryCache cache)
    {
        _deviceSource = deviceSource;
        _cache = cache;
    }

    public List<Channel> GetChannels()
    {
        return _cache.GetOrCreate(CacheName, _ =>
        {
            return _deviceSource.GetChannels();
        }) ?? [];
    }

    public void Refresh()
    {
        _cache.Remove(CacheName);
    }
}
