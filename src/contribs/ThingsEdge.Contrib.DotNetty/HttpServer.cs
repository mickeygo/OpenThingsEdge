using DotNetty.Common;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Libuv;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Codecs.Http;
using DotNetty.Handlers.Tls;

namespace ThingsEdge.Contrib.DotNetty;

/// <summary>
/// 表示是 HTTP 服务器。
/// </summary>
public sealed class HttpServer
{
    private IChannel? _bootstrapChannel;
    private int _eventLoopEvent = Environment.ProcessorCount * 2;

    public HttpServer() : this(Environment.ProcessorCount * 2)
    {
        
    }

    public HttpServer(int eventLoopEvent)
    {
        _eventLoopEvent = eventLoopEvent;

        // 禁用资源泄露检测。
        ResourceLeakDetector.Level = ResourceLeakDetector.DetectionLevel.Disabled;
    }

    /// <summary>
    /// 启动运行服务。
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public async Task RunServerAsync(ServerOptions options)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        }

        IEventLoopGroup group;
        IEventLoopGroup workGroup;
        if (options.UseLibuv)
        {
            var dispatcher = new DispatcherEventLoopGroup();
            group = dispatcher;
            workGroup = new WorkerEventLoopGroup(dispatcher);
        }
        else
        {
            group = new MultithreadEventLoopGroup(_eventLoopEvent);
            workGroup = new MultithreadEventLoopGroup();
        }

        X509Certificate2? tlsCertificate = null;
        if (options.IsSsl && !string.IsNullOrEmpty(options.CertFileName))
        {
            tlsCertificate = new X509Certificate2(options.CertFileName, options.CertPassword);
        }

        try
        {
            var bootstrap = new ServerBootstrap();
            bootstrap.Group(group, workGroup);

            if (options.UseLibuv)
            {
                bootstrap.Channel<TcpServerChannel>();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    bootstrap
                        .Option(ChannelOption.SoReuseport, true)
                        .ChildOption(ChannelOption.SoReuseaddr, true);
                }
            }
            else
            {
                bootstrap.Channel<TcpServerSocketChannel>();
            }

            bootstrap
                    .Option(ChannelOption.SoBacklog, 8192)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        if (tlsCertificate != null)
                        {
                            pipeline.AddLast(TlsHandler.Server(tlsCertificate));
                        }
                        pipeline.AddLast("encoder", new HttpResponseEncoder());
                        pipeline.AddLast("decoder", new HttpRequestDecoder(4096, 8192, 8192, false));
                        pipeline.AddLast("handler", new InternalHttpServerHandler());
                    }));

            _bootstrapChannel = await bootstrap.BindAsync(IPAddress.IPv6Any, options.Port);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            group.ShutdownGracefullyAsync().Wait();
        }
    }

    public async Task CloseAsync()
    {
        if (_bootstrapChannel != null)
        {
            await _bootstrapChannel.CloseAsync();
        }
    }
}
