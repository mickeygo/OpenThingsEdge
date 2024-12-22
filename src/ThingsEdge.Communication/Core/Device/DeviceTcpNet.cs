using System.Net.NetworkInformation;
using ThingsEdge.Communication.Core.Pipe;

namespace ThingsEdge.Communication.Core.Device;

/// <summary>
/// 基于TCP管道的设备基类信息。
/// </summary>
public abstract class DeviceTcpNet : DeviceCommunication
{
    private readonly Lazy<Ping> _ping = new(() => new Ping());

    /// <summary>
    /// 获取主机地址
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// 获取端口
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// 是否为 Socket 的异常，出现该异常时 Socket 会被关闭。
    /// </summary>
    public bool IsSocketError => ((PipeTcpNet)NetworkPipe)?.IsSocketError ?? false;

    /// <summary>
    /// Socket 在异常时关闭的委托对象，其中参数为错误状态码。
    /// </summary>
    public Action<int>? SocketErrorAndClosedDelegate
    {
        get => ((PipeTcpNet)NetworkPipe).SocketErrorAndClosedDelegate;
        set => ((PipeTcpNet)NetworkPipe).SocketErrorAndClosedDelegate = value;
    }

    /// <summary>
    /// 指定IP地址以及端口号信息来初始化对象
    /// </summary>
    /// <param name="host">远程主机IP地址</param>
    /// <param name="port">设备方的端口号信息</param>
    /// <param name="options">创建选项</param>
    public DeviceTcpNet(string host, int port, DeviceTcpNetOptions? options = null)
    {
        Host = host;
        Port = port;

        options ??= new()
        {
            SocketPoolSize = 1,
            ConnectTimeout = 3_000,
            KeepAliveTime = 60_000,
        };
        NetworkPipe = new PipeTcpNet(host, port, options);
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
            return (await _ping.Value.SendPingAsync(Host, timeout).ConfigureAwait(false)).Status == IPStatus.Success;
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
        var open = await NetworkPipe.CreateAndConnectPipeAsync().ConfigureAwait(false);
        if (!open.IsSuccess)
        {
            return open;
        }

        return await InitializationOnConnectAsync().ConfigureAwait(false);
    }

    protected override async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(NetworkPipeBase pipe, byte[] send, bool hasResponseData, bool usePackAndUnpack)
    {
        var pipeRet = await ((PipeTcpNet)pipe).CreateCopyAsync().ConfigureAwait(false);
        if (!pipeRet.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(pipeRet);
        }

        using var pipe2 = pipeRet.Content; // 必须释放以回收连接
        return await base.ReadFromCoreServerAsync(pipe2, send, hasResponseData, usePackAndUnpack).ConfigureAwait(false);
    }

    /// <summary>
    /// 关闭连接，关闭前会先执行断开连接前与服务器的处理事项。
    /// </summary>
    /// <remarks>关闭后连接驱动不可再用，若要使用需重新创建。</remarks>
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
