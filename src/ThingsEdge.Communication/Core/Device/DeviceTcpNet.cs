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
    /// 是否为 Socket 的异常，出现该异常时 Socket 会被关闭。
    /// </summary>
    public bool IsSocketError => _pipeTcpNet?.IsSocketError ?? false;

    /// <summary>
    /// Socket 异常关闭时的委托对象，其中参数为错误状态码。
    /// </summary>
    public Action<int>? SocketErrorClosedDelegate
    {
        get => _pipeTcpNet.SocketErrorClosedDelegate;
        set => _pipeTcpNet.SocketErrorClosedDelegate = value;
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
    /// <param name="timeout">超时时间</param>
    /// <returns>返回PING的结果</returns>
    public async Task<bool> PingSuccessfulAsync(int timeout)
    {
        try
        {
            return (await _ping.Value.SendPingAsync(IpAddress, timeout).ConfigureAwait(false)).Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 尝试连接远程的服务器，连接成功后会进行初始化工作（若协议有重写数据化方法）。
    /// </summary>
    /// <remarks>注意：每次执行连接都会创建一个新的管道信息。</remarks>
    /// <returns>返回连接是否和初始化成功</returns>
    public async Task<OperateResult> ConnectServerAsync()
    {
        NetworkPipe.ClosePipe();
        var open = await NetworkPipe.CreateAndConnectPipeAsync().ConfigureAwait(false);
        if (!open.IsSuccess)
        {
            return open;
        }

        return await InitializationOnConnectAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// 关闭连接，关闭前会先执行断开连接前与服务器的处理事项。
    /// </summary>
    /// <returns></returns>
    public async Task CloseAsync()
    {
        await ExtraOnDisconnectAsync().ConfigureAwait(false);
        Close();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"DeviceTcpNet<{ByteTransform}>{{{NetworkPipe}}}";
    }
}
