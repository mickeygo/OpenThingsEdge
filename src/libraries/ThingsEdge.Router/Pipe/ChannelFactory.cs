using System.Threading.Channels;
using ThingsEdge.Router.Model;

namespace ThingsEdge.Router.Pipe;

/// <summary>
/// Channel 工厂。
/// </summary>
public static class ChannelFactory
{
    /// <summary>
    /// 日志 Channel。
    /// </summary>
    public static readonly ChannelWrapper<LoggingMessage> LoggingChannel = new(System.Threading.Channels.Channel.CreateBounded<LoggingMessage>(new BoundedChannelOptions(1024)
    {
        FullMode = BoundedChannelFullMode.DropOldest,
        SingleReader = false,
        SingleWriter = false,
    }));
}
