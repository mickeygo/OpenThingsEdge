namespace ThingsEdge.Router.Devices;

internal sealed class DefaultDeviceManager : IDeviceManager
{
    public const string CacheName = "__ThingsEdge.Device.Cache";

    private readonly IDeviceProvider _deviceSource;
    private readonly IMemoryCache _cache;

    public DefaultDeviceManager(IDeviceProvider deviceSource, IMemoryCache cache)
    {
        _deviceSource = deviceSource;
        _cache = cache;
    }

    public List<Channel> GetChannels()
    {
        return _cache.GetOrCreate(CacheName, _ =>
        {
            return _deviceSource.GetChannels();
        }) ?? new(0);
    }

    public void AddChannel(Channel channel)
    {
        Refresh();
        throw new NotImplementedException();
    }

    public void AddDevice(string channelId, Device device)
    {
        Refresh();
        throw new NotImplementedException();
    }

    public void AddDeviceTag(string tagGroupId, Tag tag)
    {
        Refresh();
        throw new NotImplementedException();
    }

    public void AddTagGroup(string deviceId, TagGroup tagGroup)
    {
        Refresh();
        throw new NotImplementedException();
    }

    public void AddTag(string tagGroupId, Tag tag)
    {
        Refresh();
        throw new NotImplementedException();
    }

    public void RemoveChannel(string channelId)
    {
        Refresh();
        throw new NotImplementedException();
    }

    public void RemoveDevice(string deviceId)
    {
        Refresh();
        throw new NotImplementedException();
    }

    public void RemoveTagGroup(string tagGroupId)
    {
        Refresh();
        throw new NotImplementedException();
    }

    public void RemoveTag(string tagId)
    {
        Refresh();
        throw new NotImplementedException();
    }

    public void Refresh()
    {
        _cache.Remove(CacheName);
    }
}
