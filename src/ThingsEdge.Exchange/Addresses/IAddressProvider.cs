using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Addresses;

/// <summary>
/// 地址数据提供来源。
/// </summary>
public interface IAddressProvider
{
    /// <summary>
    /// 获取通道数据。
    /// </summary>
    /// <returns></returns>
    List<Channel> GetChannels();
}
