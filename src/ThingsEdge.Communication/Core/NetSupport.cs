using System.Net;
using System.Net.Sockets;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Exceptions;

namespace ThingsEdge.Communication.Core;

/// <summary>
/// 静态的方法支持类，提供一些网络的静态支持，支持从套接字从同步接收指定长度的字节数据。
/// </summary>
/// <remarks>
/// 在接收指定数量的字节数据的时候，如果一直接收不到，就会发生假死的状态。接收的数据时保存在内存里的，不适合大数据块的接收。
/// </remarks>
public static class NetSupport
{
    /// <summary>
    /// 表示Socket发生异常的错误码。
    /// </summary>
    public static int SocketErrorCode { get; } = -1;

    /// <summary>
    /// 创建一个新的socket对象并连接到远程的地址，需要指定远程终结点，超时时间（单位是毫秒）。
    /// </summary>
    /// <param name="endPoint">连接的目标终结点</param>
    /// <param name="timeOut">连接的超时时间</param>
    /// <returns>返回套接字的封装结果对象</returns>
    internal static async Task<OperateResult<Socket>> CreateSocketAndConnectAsync(IPEndPoint endPoint, int timeOut)
    {
        Socket socket;
        try
        {
            socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }
        catch (Exception ex)
        {
            return new OperateResult<Socket>("Socket Create Exception -> " + ex.Message);
        }
        
        try
        {
            using CancellationTokenSource cts = new(timeOut);
            await socket.ConnectAsync(endPoint, cts.Token).ConfigureAwait(false);
            return OperateResult.CreateSuccessResult(socket);
        }
        catch (OperationCanceledException)
        {
            return new OperateResult<Socket>(string.Format(StringResources.Language.ConnectTimeout, endPoint, timeOut) + " ms");
        }
        catch (Exception ex2)
        {
            socket?.Close();
            return new OperateResult<Socket>("Socket Exception -> " + ex2.Message);
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
    /// 关闭指定的socket套接字对象
    /// </summary>
    /// <param name="socket">套接字对象</param>
    public static void CloseSocket(Socket? socket)
    {
        try
        {
            socket?.Close();
        }
        catch
        {
        }
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
            socket?.Close();
            return new OperateResult<byte[]>(SocketErrorCode, ex.Message);
        }
    }

    /// <summary>
    /// 接收固定长度的字节数组，允许指定超时时间，默认为60秒，当length大于0时，接收固定长度的数据内容，当length小于0时，buffer长度的缓存数据。
    /// </summary>
    /// <param name="socket">网络通讯的套接字</param>
    /// <param name="buffer">等待接收的数据缓存信息</param>
    /// <param name="offset">开始接收数据的偏移地址</param>
    /// <param name="length">准备接收的数据长度，当length大于0时，接收固定长度的数据内容，当length小于0时，接收不大于1024长度的随机数据信息</param>
    /// <param name="timeOut">单位：毫秒，超时时间，默认为60秒，如果设置小于0，则不检查超时时间</param>
    /// <returns>包含了字节数据的结果类</returns>
    public static async Task<OperateResult<int>> SocketReceiveAsync(Socket socket, byte[] buffer, int offset, int length, int timeOut = 60_000)
    {
        if (length == 0)
        {
            return OperateResult.CreateSuccessResult(length);
        }

        var hslTimeOut = CommTimeOut.HandleTimeOutCheck(socket, timeOut);
        try
        {
            int count;
            if (length > 0)
            {
                var alreadyCount = 0;
                do
                {
                    var currentReceiveLength = length - alreadyCount > 16384 ? 16384 : length - alreadyCount;
                    count = await Task.Factory.FromAsync(socket.BeginReceive(buffer, alreadyCount + offset, currentReceiveLength, SocketFlags.None, null, socket), socket.EndReceive).ConfigureAwait(false);
                    alreadyCount += count;
                    if (count > 0)
                    {
                        hslTimeOut.StartTime = DateTime.Now;
                        continue;
                    }
                    throw new RemoteClosedException();
                }
                while (alreadyCount < length);
                hslTimeOut.IsSuccessful = true;

                await socket.ReceiveAsync(buffer).ConfigureAwait(false);

                return OperateResult.CreateSuccessResult(length);
            }

            count = await Task.Factory.FromAsync(socket.BeginReceive(buffer, offset, buffer.Length - offset, SocketFlags.None, null, socket), socket.EndReceive).ConfigureAwait(false);
            if (count == 0)
            {
                throw new RemoteClosedException();
            }
            hslTimeOut.IsSuccessful = true;
            return OperateResult.CreateSuccessResult(count);
        }
        catch (RemoteClosedException)
        {
            socket?.Close();
            hslTimeOut.IsSuccessful = true;
            return new OperateResult<int>(SocketErrorCode, StringResources.Language.RemoteClosedConnection);
        }
        catch (Exception ex)
        {
            socket?.Close();
            hslTimeOut.IsSuccessful = true;
            if (hslTimeOut.IsTimeout)
            {
                return new OperateResult<int>(SocketErrorCode, StringResources.Language.ReceiveDataTimeout + hslTimeOut.DelayTime);
            }
            return new OperateResult<int>(SocketErrorCode, "Socket Exception -> " + ex.Message);
        }
    }

    /// <summary>
    /// 接收固定长度的字节数组，指定超时时间，默认为60秒。
    /// </summary>
    /// <param name="socket">网络通讯的套接字</param>
    /// <param name="buffer">等待接收的数据缓存信息</param>
    /// <param name="timeOut">单位：毫秒，超时时间，默认为60秒，如果设置小于0，则不检查超时时间</param>
    /// <returns>包含了字节数据的结果类</returns>
    public static async Task<OperateResult<int>> SocketReceiveAsync(Socket socket, byte[] buffer, int timeOut = 60_000)
    {
        try
        {
            using CancellationTokenSource cts = new(timeOut);
            var count = await socket.ReceiveAsync(buffer, cts.Token).ConfigureAwait(false);
            if (count == 0)
            {
                return new OperateResult<int>(SocketErrorCode, StringResources.Language.RemoteClosedConnection);
            }
            return OperateResult.CreateSuccessResult(count);
        }
        catch (OperationCanceledException)
        {
            socket?.Close();
            return new OperateResult<int>(SocketErrorCode, StringResources.Language.ReceiveDataTimeout + timeOut);
        }
        catch (SocketException ex)
        {
            socket?.Close();
            return new OperateResult<int>(SocketErrorCode, "Socket Exception -> " + ex.Message);
        }
    }
}
