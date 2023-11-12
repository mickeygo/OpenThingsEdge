namespace ThingsEdge.Router.Devices;

/// <summary>
/// 设备工厂。
/// </summary>
public interface IDeviceFactory
{
    /// <summary>
    /// 获取所有的通道。
    /// </summary>
    /// <returns></returns>
    List<Channel> GetChannels();

    /// <summary>
    /// 刷新数据
    /// </summary>
    void Refresh();
}
