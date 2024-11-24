using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Addresses;

/// <summary>
/// 地址工厂。
/// </summary>
public interface IAddressFactory
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
