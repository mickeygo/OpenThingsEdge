using ThingsEdge.Communication;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.ModBus;
using ThingsEdge.Communication.Profinet.AllenBradley;
using ThingsEdge.Communication.Profinet.Delta;
using ThingsEdge.Communication.Profinet.Fuji;
using ThingsEdge.Communication.Profinet.Inovance;
using ThingsEdge.Communication.Profinet.Melsec;
using ThingsEdge.Communication.Profinet.Omron;
using ThingsEdge.Communication.Profinet.Panasonic;
using ThingsEdge.Communication.Profinet.Siemens;
using ThingsEdge.Communication.Profinet.XINJE;
using ThingsEdge.Exchange.Configuration;
using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Engine.Connectors;

namespace ThingsEdge.Exchange.Connectors;

/// <summary>
/// 设备驱动连管理接器。
/// </summary>
internal sealed class DefaultDriverConnectorManager(IOptions<ExchangeOptions> options) : IDriverConnectorManager2
{
    public async Task<IDriverConnector> CreateAndConnectAsync(Device deviceInfo, CancellationToken cancellationToken = default)
    {
        DeviceTcpNetOptions netOptions = new()
        {
            SocketPoolSize = deviceInfo.PoolSize > 0 ? deviceInfo.PoolSize : options.Value.SocketPoolSize,
            ConnectTimeout = options.Value.NetworkConnectTimeout,
            KeepAliveTime = options.Value.NetworkKeepAliveTime,
        };

        var maxPDUSize = deviceInfo.MaxPDUSize > 0 ? deviceInfo.MaxPDUSize : options.Value.MaxPDUSize;
        DeviceTcpNet driverNet = deviceInfo.Model switch
        {
            DriverModel.ModbusTcp => new ModbusTcpNet(deviceInfo.Host, options: netOptions),
            DriverModel.S7_1500 => new SiemensS7Net(SiemensPLCS.S1500, deviceInfo.Host, options: netOptions) { PDUCustomLength = maxPDUSize },
            DriverModel.S7_1200 => new SiemensS7Net(SiemensPLCS.S1200, deviceInfo.Host, options: netOptions) { PDUCustomLength = maxPDUSize },
            DriverModel.S7_400 => new SiemensS7Net(SiemensPLCS.S400, deviceInfo.Host, options: netOptions) { PDUCustomLength = maxPDUSize },
            DriverModel.S7_300 => new SiemensS7Net(SiemensPLCS.S300, deviceInfo.Host, options: netOptions) { PDUCustomLength = maxPDUSize },
            DriverModel.S7_S200 => new SiemensS7Net(SiemensPLCS.S200, deviceInfo.Host, options: netOptions) { PDUCustomLength = maxPDUSize },
            DriverModel.S7_S200Smart => new SiemensS7Net(SiemensPLCS.S200Smart, deviceInfo.Host, options: netOptions) { PDUCustomLength = maxPDUSize },
            DriverModel.Melsec_MC => new MelsecMcNet(deviceInfo.Host, deviceInfo.Port, options: netOptions),
            DriverModel.Melsec_MCAscii => new MelsecMcAsciiNet(deviceInfo.Host, deviceInfo.Port, options: netOptions),
            DriverModel.Melsec_MCR => new MelsecMcRNet(deviceInfo.Host, deviceInfo.Port, options: netOptions),
            DriverModel.Melsec_A1E => new MelsecA1ENet(deviceInfo.Host, deviceInfo.Port, options: netOptions),
            DriverModel.Melsec_CIP => new MelsecCipNet(deviceInfo.Host, options: netOptions),
            DriverModel.Omron_FinsTcp => new OmronFinsNet(deviceInfo.Host, deviceInfo.Port, options: netOptions),
            DriverModel.Omron_CIP => new OmronCipNet(deviceInfo.Host, deviceInfo.Port, options: netOptions),
            DriverModel.Omron_HostLinkOverTcp => new OmronHostLinkOverTcp(deviceInfo.Host, deviceInfo.Port, options: netOptions),
            DriverModel.Omron_HostLinkCModeOverTcp => new OmronHostLinkCModeOverTcp(deviceInfo.Host, deviceInfo.Port, options: netOptions),
            DriverModel.AllenBradley_CIP => new AllenBradleyNet(deviceInfo.Host, options: netOptions),
            DriverModel.Inovance_Tcp => new InovanceTcpNet(deviceInfo.Host, options: netOptions),
            DriverModel.Delta_Tcp => new DeltaTcpNet(deviceInfo.Host, options: netOptions),
            DriverModel.Fuji_SPH => new FujiSPHNet(deviceInfo.Host, options: netOptions),
            DriverModel.Panasonic_Mc => new PanasonicMcNet(deviceInfo.Host, deviceInfo.Port, options: netOptions),
            DriverModel.XinJE_Tcp => new XinJETcpNet(deviceInfo.Host, options: netOptions),
            _ => throw new NotImplementedException("没有找到指定的设备驱动"),
        };

        DriverConnector driverConnector = new(deviceInfo.DeviceId, deviceInfo.Host, driverNet.Port, driverNet);
        if (driverConnector.Driver is DeviceTcpNet networkDevice)
        {
            driverConnector.ConnectedStatus = ConnectionStatus.Disconnected; // 初始化
            networkDevice.SocketErrorAndClosedDelegate = code =>
            {
                // 根据错误代码来判断是否断开连接
                if (networkDevice.IsSocketError)
                {
                    driverConnector.ConnectedStatus = ConnectionStatus.Disconnected;

                    if (code is (int)ErrorCode.SocketConnectException
                            or (int)ErrorCode.SocketConnectTimeoutException
                            or (int)ErrorCode.RemoteClosedConnection
                            or (int)ErrorCode.ReceiveDataTimeout
                            or (int)ErrorCode.SocketSendException
                            or (int)ErrorCode.SocketReceiveException
                            or (int)ErrorCode.SocketException)
                    {
                        // 记录 已与服务器断开 的日志
                    }
                }
            };

            // 先检查服务器能否访问
            try
            {
                if (await networkDevice.PingSuccessfulAsync(1_000).ConfigureAwait(false))
                {
                    driverConnector.Available = true;
                    var ret = await networkDevice.ConnectServerAsync().ConfigureAwait(false);
                    if (ret.IsSuccess)
                    {
                        driverConnector.ConnectedStatus = ConnectionStatus.Connected;
                    }
                    else
                    {
                        // 记录 尝试连接服务失败 日志
                    }
                }
                else
                {
                    // 记录 尝试 Ping 服务失败 日志
                }
            }
            catch (Exception)
            {
                // 记录 尝试连接服务异常 日志
            }
        }

        return driverConnector;
    }
}
