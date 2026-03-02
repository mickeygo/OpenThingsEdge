using Microsoft.Extensions.Hosting;
using ThingsEdge.Exchange;
using ThingsEdge.Silos.Client.Forwarders;

namespace ThingsEdge.Silos.Client.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IHostBuilder hostBuilder)
    {
        /// <summary>
        /// 添加 Silos 客户端服务。
        /// </summary>
        public void AddSilosClient()
        {
        }

        /// <summary>
        /// 注册 gRPC 边缘服务
        /// </summary>
        public void AddGrpcSilos()
        {
            hostBuilder.ConfigureServices((_, services) =>
            {
                services.AddGrpcClient<Greeter.GreeterClient>(options =>
                {
                    options.Address = new Uri("http://localhost:5001");
                });
            });
        }

        /// <summary>
        /// 注册边缘服务
        /// </summary>
        public void AddEdgeSilos()
        {
            hostBuilder.AddThingsEdgeExchange(static builder =>
                builder.UseDeviceFileProvider() // UseDeviceCustomProvider<ModbusTcpAddressProvider>()
                    .UseDeviceHeartbeatForwarder<HeartbeatForwarder>()
                    .UseNativeNoticeForwarder<NoticeForwarder>()
                    .UseNativeTriggerForwarder<TriggerForwader>()
                    .UseNativeSwitchForwarder<SwitchForwarder>()
                    .UseOptions(options =>
                    {
                        options.SocketPoolSize = 5;
                        options.TriggerStateWriteTagUseOther = true;
                        options.Curve.FileType = Exchange.Configuration.CurveFileExt.CSV;
                    })
            );
        }
    }
}
