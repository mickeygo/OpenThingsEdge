namespace ThingsEdge.Router.Devices;

/// <summary>
/// 设备管理。
/// </summary>
public interface IDeviceManager
{
    /// <summary>
    /// 获取所有的通道。
    /// </summary>
    /// <returns></returns>
    List<Channel> GetChannels();

    /// <summary>
    /// 添加通道。
    /// </summary>
    /// <param name="channel">通道。</param>
    void AddChannel(Channel channel);

    /// <summary>
    /// 添加设备。
    /// </summary>
    /// <param name="channelId">通道 Id。</param>
    /// <param name="device">设备。</param>
    void AddDevice(string channelId, Device device);

    /// <summary>
    /// 添加设备标记。
    /// </summary>
    /// <param name="tagGroupId">标记组Id。</param>
    /// <param name="tag">标记。</param>
    void AddDeviceTag(string tagGroupId, Tag tag);

    /// <summary>
    /// 添加标记组。
    /// </summary>
    /// <param name="deviceId">设备Id。</param>
    /// <param name="tagGroup">标记组。</param>
    void AddTagGroup(string deviceId, TagGroup tagGroup);

    /// <summary>
    /// 添加标记。
    /// </summary>
    /// <param name="tagGroupId">标记组Id。</param>
    /// <param name="tag">标记。</param>
    void AddTag(string tagGroupId, Tag tag);

    /// <summary>
    /// 移除通道。
    /// </summary>
    /// <param name="channelId">要移除的通道Id。</param>
    void RemoveChannel(string channelId);

    /// <summary>
    /// 移除设备。
    /// </summary>
    /// <param name="deviceId">设备Id。</param>
    void RemoveDevice(string deviceId);

    /// <summary>
    /// 移除标记组。
    /// </summary>
    /// <param name="tagGroupId">标记组Id。</param>
    void RemoveTagGroup(string tagGroupId);

    /// <summary>
    /// 移除标记。
    /// </summary>
    /// <param name="tagId">标记Id。</param>
    void RemoveTag(string tagId);

    /// <summary>
    /// 刷新数据。
    /// </summary>
    void Refresh();
}
