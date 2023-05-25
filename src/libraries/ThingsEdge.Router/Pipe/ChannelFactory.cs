using System.Threading.Channels;

namespace ThingsEdge.Router.Pipe;

/// <summary>
/// Channel 工厂。
/// </summary>
public static class ChannelFactory
{
    public static readonly Channel<string> Channel = System.Threading.Channels.Channel.CreateBounded<string>(new BoundedChannelOptions(1024)
    {
        FullMode = BoundedChannelFullMode.DropOldest,
        SingleReader = false,
        SingleWriter = false,
    });
}
