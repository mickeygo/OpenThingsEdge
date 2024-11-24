using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Addresses;

internal sealed class DefaultAddressFactory : IAddressFactory
{
    public const string CacheName = "__ThingsEdge.Device.Cache";

    private readonly IAddressProvider _deviceSource;
    private readonly IMemoryCache _cache;

    public DefaultAddressFactory(IAddressProvider deviceSource, IMemoryCache cache)
    {
        _deviceSource = deviceSource;
        _cache = cache;
    }

    public List<Channel> GetChannels()
    {
        return _cache.GetOrCreate(CacheName, _ => _deviceSource.GetChannels()) ?? [];
    }

    public void Refresh()
    {
        _cache.Remove(CacheName);
    }
}
