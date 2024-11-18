using System.Net.NetworkInformation;
using ThingsEdge.Communication.Core.Pipe;

namespace ThingsEdge.Communication.Core.Device;

/// <summary>
/// 基于TCP管道的设备基类信息。
/// </summary>
public abstract class DeviceTcpNet : DeviceCommunication
{
    private readonly PipeTcpNet _pipeTcpNet;

    private readonly Lazy<Ping> _ping = new(() => new Ping());

    public virtual int ConnectTimeOut
    {
        get
        {
            if (CommunicationPipe is not PipeTcpNet { ConnectTimeOut: var connectTimeOut })
            {
                return _pipeTcpNet.ConnectTimeOut;
            }
            return connectTimeOut;
        }
        set
        {
            if (value >= 0 && CommunicationPipe is PipeTcpNet pipeTcpNet)
            {
                pipeTcpNet.ConnectTimeOut = value;
            }
        }
    }

    public virtual string IpAddress
    {
        get
        {
            if (!(CommunicationPipe is PipeTcpNet { IpAddress: var ipAddress }))
            {
                return _pipeTcpNet.IpAddress;
            }
            return ipAddress;
        }
        set
        {
            if (CommunicationPipe is PipeTcpNet pipeTcpNet)
            {
                pipeTcpNet.IpAddress = value;
            }
        }
    }

    /// <inheritdoc />
    public virtual int Port
    {
        get
        {
            if (CommunicationPipe is not PipeTcpNet { Port: var port })
            {
                return _pipeTcpNet.Port;
            }
            return port;
        }
        set
        {
            if (CommunicationPipe is PipeTcpNet pipeTcpNet)
            {
                pipeTcpNet.Port = value;
            }
        }
    }

    /// <summary>
    /// 实例化一个默认的对象
    /// </summary>
    public DeviceTcpNet() : this("127.0.0.1", 5000)
    {
    }

    /// <summary>
    /// 指定IP地址以及端口号信息来初始化对象。
    /// </summary>
    /// <param name="ipAddress">IP地址信息，可以是IPv4, IPv6, 也可以是域名</param>
    /// <param name="port">设备方的端口号信息</param>
    public DeviceTcpNet(string ipAddress, int port)
    {
        _pipeTcpNet = new PipeTcpNet
        {
            IpAddress = ipAddress,
            Port = port
        };
        CommunicationPipe = _pipeTcpNet;
    }

    /// <summary>
    /// 对当前设备的IP地址进行PING的操作，返回PING的结果，正常来说，返回<see cref="F:System.Net.NetworkInformation.IPStatus.Success" />。
    /// </summary>
    /// <returns>返回PING的结果</returns>
    public IPStatus IpAddressPing()
    {
        return _ping.Value.Send(IpAddress).Status;
    }

    /// <summary>
    /// 尝试连接远程的服务器，如果连接成功，就切换短连接模式到长连接模式，后面的每次请求都共享一个通道，使得通讯速度更快速。
    /// </summary>
    /// <returns>返回连接结果，如果失败的话（也即IsSuccess为False），包含失败信息</returns>
    public async Task<OperateResult> ConnectServerAsync()
    {
        await CommunicationPipe.CloseCommunicationAsync().ConfigureAwait(continueOnCapturedContext: false);
        var open = await CommunicationPipe.OpenCommunicationAsync().ConfigureAwait(continueOnCapturedContext: false);
        if (!open.IsSuccess)
        {
            return open;
        }

        if (open.Content)
        {
            return await InitializationOnConnectAsync().ConfigureAwait(continueOnCapturedContext: false);
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 手动断开与远程服务器的连接，如果当前是长连接模式，那么就会切换到短连接模式。
    /// </summary>
    /// <returns>关闭连接，不需要查看IsSuccess属性查看</returns>
    /// <example>
    /// 直接关闭连接即可，基本上是不需要进行成功的判定。
    /// </example>
    public OperateResult CloseConnect()
    {
        var operateResult = ExtraOnDisconnect();
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }

        return CommunicationPipe.CloseCommunication();
    }

    /// <summary>
    /// 手动断开与远程服务器的连接，如果当前是长连接模式，那么就会切换到短连接模式。
    /// </summary>
    /// <returns>关闭连接，不需要查看IsSuccess属性查看</returns>
    /// <example>
    /// 直接关闭连接即可，基本上是不需要进行成功的判定。
    /// </example>
    public async Task<OperateResult> CloseConnectAsync()
    {
        var result = await ExtraOnDisconnectAsync().ConfigureAwait(continueOnCapturedContext: false);
        if (!result.IsSuccess)
        {
            return result;
        }

        return await CommunicationPipe.CloseCommunicationAsync().ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"DeviceTcpNet<{ByteTransform}>{{{CommunicationPipe}}}";
    }
}
