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

    /// <summary>
    /// 获取地址
    /// </summary>
    public string IpAddress => _pipeTcpNet.IpAddress;

    /// <summary>
    /// 获取端口
    /// </summary>
    public int Port => _pipeTcpNet.Port;

    /// <summary>
    /// 连接服务器超时时间。
    /// </summary>
    public int ConnectTimeout
    {
        get => _pipeTcpNet.ConnectTimeout;
        set => _pipeTcpNet.ConnectTimeout = value;
    }

    /// <summary>
    /// 获取或设置 Socket 保活时长，只有在大于 0 时才启用，单位 ms。
    /// </summary>
    public int KeepAliveTime
    {
        get => _pipeTcpNet.KeepAliveTime;
        set => _pipeTcpNet.KeepAliveTime = value;
    }

    /// <summary>
    /// 指定IP地址以及端口号信息来初始化对象。
    /// </summary>
    /// <param name="ipAddress">IP地址信息，可以是IPv4, IPv6, 也可以是域名</param>
    /// <param name="port">设备方的端口号信息</param>
    public DeviceTcpNet(string ipAddress, int port)
    {
        NetworkPipe = _pipeTcpNet = new PipeTcpNet(ipAddress, port);
    }

    /// <summary>
    /// 对当前设备的IP地址进行 PING 的操作，若返回 <see cref="IPStatus.Success" /> 表示成功，其他或异常为失败。
    /// </summary>
    /// <returns>返回PING的结果</returns>
    public bool PingSuccessful()
    {
        try
        {
            return _ping.Value.Send(IpAddress).Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 尝试连接远程的服务器，如果连接成功，就切换短连接模式到长连接模式，后面的每次请求都共享一个通道，使得通讯速度更快速。
    /// </summary>
    /// <returns>返回连接结果，如果失败的话（也即IsSuccess为False），包含失败信息</returns>
    public async Task<OperateResult> ConnectServerAsync()
    {
        NetworkPipe.CloseCommunication();
        var open = await NetworkPipe.OpenCommunicationAsync().ConfigureAwait(false);
        if (!open.IsSuccess)
        {
            return open;
        }

        if (open.Content)
        {
            return await InitializationOnConnectAsync().ConfigureAwait(false);
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
        OnExtraOnDisconnect?.Invoke(ConnectionId);
        return NetworkPipe.CloseCommunication();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"DeviceTcpNet<{ByteTransform}>{{{NetworkPipe}}}";
    }
}
