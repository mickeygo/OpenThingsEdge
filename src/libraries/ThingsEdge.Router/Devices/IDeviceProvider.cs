namespace ThingsEdge.Router.Devices;

/// <summary>
/// 设备数据提供来源。
/// </summary>
public interface IDeviceProvider
{
    /// <summary>
    /// 获取通道数据。
    /// </summary>
    /// <returns></returns>
    List<Channel> GetChannels();
}
