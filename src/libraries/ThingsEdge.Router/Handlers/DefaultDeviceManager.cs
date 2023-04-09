using ThingsEdge.Common.Storage;

namespace ThingsEdge.Router.Handlers;

internal sealed class DefaultDeviceManager : IDeviceManager
{
    public const string CacheName = "ThingsEdge.Device";

    private readonly IMemoryCache _cache;

    public DefaultDeviceManager(IMemoryCache cache)
    {
        _cache = cache;
    }

    public List<DeviceInfo> GetAll()
    {
        using var db = new LiteDbManager().Create();
        throw new NotImplementedException();
    }

    public Task<List<DeviceInfo>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<DeviceInfo?> GetAsync(string name)
    {
        throw new NotImplementedException();
    }

    public Task<(bool ok, string err)> AddAsync(DeviceInfo deviceInfo)
    {
        throw new NotImplementedException();
    }

    public (bool ok, string err) Update(DeviceInfo deviceInfo)
    {
        throw new NotImplementedException();
    }

    public void Remove(DeviceInfo deviceInfo)
    {
        _cache.Remove(CacheName);
    }
}
