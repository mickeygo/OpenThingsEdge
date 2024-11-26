using System.Net;
using System.Net.Sockets;
using ThingsEdge.Communication.Common;

namespace ThingsEdge.Communication.Core;

/// <summary>
/// 静态的方法支持类，提供一些网络的静态支持，支持从套接字从同步接收指定长度的字节数据。
/// </summary>
/// <remarks>
/// 在接收指定数量的字节数据的时候，如果一直接收不到，就会发生假死的状态。接收的数据时保存在内存里的，不适合大数据块的接收。
/// </remarks>
internal static class NetSupport
{
    /// <summary>
    /// 创建一个新的 socket 对象并连接到远程的地址，需要指定远程终结点，超时时间（单位：毫秒）。
    /// </summary>
    /// <param name="endPoint">连接的目标终结点</param>
    /// <param name="timeout">连接的超时时间</param>
    /// <returns>返回套接字的封装结果对象</returns>
    internal static async Task<OperateResult<Socket>> CreateSocketAndConnectAsync(IPEndPoint endPoint, int timeout)
    {
        Socket socket = default!;
        try
        {
            socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // 设置超时
            using CancellationTokenSource cts = new(timeout);
            await socket.ConnectAsync(endPoint, cts.Token).ConfigureAwait(false);
            return OperateResult.CreateSuccessResult(socket);
        }
        catch (OperationCanceledException)
        {
            socket.SafeClose();
            return new OperateResult<Socket>((int)CommErrorCode.SocketConnectTimeoutException, string.Format(StringResources.Language.ConnectTimeout, endPoint, timeout) + " ms");
        }
        catch (SocketException ex)
        {
            socket.SafeClose();
            return new OperateResult<Socket>((int)CommErrorCode.SocketConnectException, $"Socket Connect Exception -> {ex.Message}");
        }
    }

    /// <inheritdoc />
    public static async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(IEnumerable<byte[]> send, Func<byte[], Task<OperateResult<byte[]>>> funcRead)
    {
        var array = new List<byte>();
        foreach (var data in send)
        {
            var read = await funcRead(data).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return read;
            }

            if (read.Content != null)
            {
                array.AddRange(read.Content);
            }
        }
        return OperateResult.CreateSuccessResult(array.ToArray());
    }

    /// <summary>
    /// 发送消息给套接字，直到完成的时候返回，经过测试，本方法是线程安全的。
    /// </summary>
    /// <param name="socket">网络套接字</param>
    /// <param name="data">要发送的字节数据</param>
    /// <returns>发送是否成功的结果</returns>
    public static async Task<OperateResult> SocketSendAsync(Socket socket, byte[] data)
    {
        try
        {
            await socket.SendAsync(data).ConfigureAwait(false);
            return OperateResult.CreateSuccessResult();
        }
        catch (SocketException ex)
        {
            socket.SafeClose();
            return new OperateResult<byte[]>((int)CommErrorCode.SocketSendException, ex.Message);
        }
    }

    /// <summary>
    /// 接收固定长度的字节数组，当length大于0时，接收固定长度的数据内容，当length小于0时，buffer长度的缓存数据。
    /// </summary>
    /// <param name="socket">网络通讯的套接字</param>
    /// <param name="buffer">等待接收的数据缓存信息</param>
    /// <param name="offset">开始接收数据的偏移地址</param>
    /// <param name="length">准备接收的数据长度，当length大于0时，接收固定长度的数据内容，当length小于0时，接收不大于1024长度的随机数据信息</param>
    /// <param name="timeout">超时时间，单位：毫秒，超时时间</param>
    /// <returns>包含了字节数据的结果类</returns>
    public static async Task<OperateResult<int>> SocketReceiveAsync(Socket socket, byte[] buffer, int offset, int length, int timeout)
    {
        if (length == 0)
        {
            return OperateResult.CreateSuccessResult(length);
        }

        if (length > 0)
        {
            return await SocketReceiveAsync(socket, new ArraySegment<byte>(buffer, offset, length), timeout).ConfigureAwait(false);
        }

        // length 小于 0 时，按整个 buffer 读取
        return await SocketReceiveAsync(socket, buffer, timeout).ConfigureAwait(false);
    }

    /// <summary>
    /// 接收固定长度的字节数组，指定超时时间，默认为60秒。
    /// </summary>
    /// <param name="socket">网络通讯的套接字</param>
    /// <param name="buffer">等待接收的数据缓存信息</param>
    /// <param name="timeout">超时时间，单位：毫秒</param>
    /// <returns>包含了字节数据的结果类</returns>
    public static async Task<OperateResult<int>> SocketReceiveAsync(Socket socket, ArraySegment<byte> buffer, int timeout)
    {
        try
        {
            using CancellationTokenSource cts = new(timeout);
            var count = await socket.ReceiveAsync(buffer, cts.Token).ConfigureAwait(false);
            if (count == 0)
            {
                socket.SafeClose();
                return new OperateResult<int>((int)CommErrorCode.RemoteClosedConnection, StringResources.Language.RemoteClosedConnection);
            }
            return OperateResult.CreateSuccessResult(count);
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("超时取消，关闭 Socket");
            socket.SafeClose();
            return new OperateResult<int>((int)CommErrorCode.ReceiveDataTimeout, StringResources.Language.ReceiveDataTimeout + timeout);
        }
        catch (SocketException ex)
        {
            Debug.WriteLine($"Socket异常，关闭 Socket，{ex.Message}");
            socket.SafeClose();
            return new OperateResult<int>((int)CommErrorCode.SocketException, "Socket Exception -> " + ex.Message);
        }
    }
}
