using System.Threading.Channels;

namespace ThingsEdge.Router.Management;

/// <summary>
/// Channel 工厂。
/// </summary>
public sealed class ChannelFactory
{
    private ChannelFactory()
    {
        
    }

    /// <summary>
    /// 用于日志的 Channel。
    /// </summary>
    public static Channel<string> LoggingChannel => Channel.CreateBounded<string>(new BoundedChannelOptions(128)
    {
        FullMode = BoundedChannelFullMode.DropOldest,
    });
}
