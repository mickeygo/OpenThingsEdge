using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using ThingsEdge.Communication.Exceptions;
using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Core;

/// <summary>
/// 静态的方法支持类，提供一些网络的静态支持，支持从套接字从同步接收指定长度的字节数据，并支持报告进度。
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
    /// 从socket的网络中读取数据内容，需要指定数据长度和超时的时间，为了防止数据太大导致接收失败，所以此处接收到新的数据之后就更新时间。
    /// </summary>
    /// <param name="socket">网络套接字</param>
    /// <param name="buffer">缓存的字节数组</param>
    /// <param name="offset">偏移信息</param>
    /// <param name="length">接收长度</param>
    /// <exception cref="RemoteCloseException">远程关闭的异常信息</exception>
    internal static void ReceiveBytesFromSocket(Socket socket, byte[] buffer, int offset, int length)
    {
        var num = 0;
        while (num < length)
        {
            var size = Math.Min(length - num, 16384);
            var num2 = socket.Receive(buffer, num + offset, size, SocketFlags.None);
            num += num2;
            if (num2 == 0)
            {
                throw new RemoteCloseException();
            }
        }
    }

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
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
        }
        catch (Exception ex)
        {
            return new OperateResult<Socket>("Socket Create Exception -> " + ex.Message);
        }

        using CancellationTokenSource cts = new(timeOut);
        try
        {
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
    public static void CloseSocket(Socket socket)
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
    /// 创建接收数据的缓存，并返回是否创建成功。
    /// </summary>
    /// <param name="length">准备创建的长度信息，如果传入负数，则自动创建长度 2048 的缓存</param>
    /// <returns>创建成功的缓存</returns>
    public static OperateResult<byte[]> CreateReceiveBuffer(int length)
    {
        var num = length >= 0 ? length : 2048;
        try
        {
            return OperateResult.CreateSuccessResult(new byte[num]);
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>($"Create byte[{num}] buffer failed: {ex.Message}");
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
    public static OperateResult<int> SocketReceive(Socket socket, byte[] buffer, int offset, int length, int timeOut = 60000)
    {
        if (length == 0)
        {
            return OperateResult.CreateSuccessResult(0);
        }

        try
        {
            socket.ReceiveTimeout = timeOut;
            if (length > 0)
            {
                ReceiveBytesFromSocket(socket, buffer, offset, length);
                return OperateResult.CreateSuccessResult(length);
            }
            var num = socket.Receive(buffer, offset, buffer.Length - offset, SocketFlags.None);
            if (num == 0)
            {
                throw new RemoteCloseException();
            }
            return OperateResult.CreateSuccessResult(num);
        }
        catch (RemoteCloseException)
        {
            return new OperateResult<int>(SocketErrorCode, StringResources.Language.RemoteClosedConnection ?? "");
        }
        catch (SocketException ex2)
        {
            if (ex2.SocketErrorCode == SocketError.TimedOut)
            {
                return new OperateResult<int>(SocketErrorCode, $"Socket Exception -> {ex2.Message} Timeout: {timeOut}");
            }
            return new OperateResult<int>(SocketErrorCode, "Socket Exception -> " + ex2.Message);
        }
        catch (Exception ex3)
        {
            return new OperateResult<int>(SocketErrorCode, "Exception -> " + ex3.Message);
        }
    }

    /// <summary>
    /// 接收固定长度的字节数组，允许指定超时时间，默认为60秒，当length大于0时，接收固定长度的数据内容，当length小于0时，接收不大于2048长度的随机数据信息。
    /// </summary>
    /// <param name="socket">网络通讯的套接字<br />Network communication socket</param>
    /// <param name="length">准备接收的数据长度，当length大于0时，接收固定长度的数据内容，当length小于0时，接收不大于1024长度的随机数据信息</param>
    /// <param name="timeOut">单位：毫秒，超时时间，默认为60秒，如果设置小于0，则不检查超时时间</param>
    /// <returns>包含了字节数据的结果类</returns>
    public static async Task<OperateResult<byte[]>> SocketReceiveAsync(Socket socket, int length, int timeOut = 60000)
    {
        if (length == 0)
        {
            return OperateResult.CreateSuccessResult(Array.Empty<byte>());
        }

        var createBuffer = CreateReceiveBuffer(length);
        if (!createBuffer.IsSuccess)
        {
            return createBuffer;
        }

        var receive = await SocketReceiveAsync(socket, createBuffer.Content!, 0, length, timeOut).ConfigureAwait(false);
        if (!receive.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(receive);
        }
        return OperateResult.CreateSuccessResult(length > 0 ? createBuffer.Content! : createBuffer.Content!.SelectBegin(receive.Content));
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
    public static async Task<OperateResult<int>> SocketReceiveAsync(Socket socket, byte[] buffer, int offset, int length, int timeOut = 60000)
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
                    throw new RemoteCloseException();
                }
                while (alreadyCount < length);
                hslTimeOut.IsSuccessful = true;

                await socket.ReceiveAsync(buffer).ConfigureAwait(false);

                return OperateResult.CreateSuccessResult(length);
            }
            count = await Task.Factory.FromAsync(socket.BeginReceive(buffer, offset, buffer.Length - offset, SocketFlags.None, null, socket), socket.EndReceive).ConfigureAwait(false);
            if (count == 0)
            {
                throw new RemoteCloseException();
            }
            hslTimeOut.IsSuccessful = true;
            return OperateResult.CreateSuccessResult(count);
        }
        catch (RemoteCloseException)
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

    /// <inheritdoc />
    public static OperateResult<int> SocketReceive(SslStream ssl, byte[] buffer, int offset, int length, int timeOut = 60000, Action<long, long>? reportProgress = null)
    {
        if (length == 0)
        {
            return OperateResult.CreateSuccessResult(0);
        }

        try
        {
            ssl.ReadTimeout = timeOut;
            if (length > 0)
            {
                var num = 0;
                while (num < length)
                {
                    var count = Math.Min(length - num, 16384);
                    var num2 = ssl.Read(buffer, num + offset, count);
                    num += num2;
                    if (num2 == 0)
                    {
                        throw new RemoteCloseException();
                    }
                    reportProgress?.Invoke(num, length);
                }
                return OperateResult.CreateSuccessResult(length);
            }
            var num3 = ssl.Read(buffer, offset, buffer.Length - offset);
            if (num3 == 0)
            {
                throw new RemoteCloseException();
            }
            return OperateResult.CreateSuccessResult(num3);
        }
        catch (RemoteCloseException)
        {
            ssl?.Close();
            return new OperateResult<int>(SocketErrorCode, "Socket Exception -> " + StringResources.Language.RemoteClosedConnection);
        }
        catch (Exception ex2)
        {
            ssl?.Close();
            return new OperateResult<int>(SocketErrorCode, "Socket Exception -> " + ex2.Message);
        }
    }

    /// <inheritdoc />
    public static async Task<OperateResult<byte[]>> SocketReceiveAsync(SslStream ssl, int length, int timeOut = 60000, Action<long, long>? reportProgress = null)
    {
        if (length == 0)
        {
            return OperateResult.CreateSuccessResult(Array.Empty<byte>());
        }
        var createBuffer = CreateReceiveBuffer(length);
        if (!createBuffer.IsSuccess)
        {
            return createBuffer;
        }
        var receive = await SocketReceiveAsync(ssl, createBuffer.Content, 0, length, timeOut, reportProgress).ConfigureAwait(false);
        if (!receive.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(receive);
        }
        return OperateResult.CreateSuccessResult(length > 0 ? createBuffer.Content : createBuffer.Content.SelectBegin(receive.Content));
    }

    /// <inheritdoc />
    public static async Task<OperateResult<int>> SocketReceiveAsync(SslStream ssl, byte[] buffer, int offset, int length, int timeOut = 60000, Action<long, long>? reportProgress = null)
    {
        if (length == 0)
        {
            return OperateResult.CreateSuccessResult(length);
        }

        try
        {
            int count;
            if (length > 0)
            {
                var alreadyCount = 0;
                do
                {
                    var currentReceiveLength = length - alreadyCount > 16384 ? 16384 : length - alreadyCount;
                    count = await ssl.ReadAsync(buffer, alreadyCount + offset, currentReceiveLength).ConfigureAwait(false);
                    alreadyCount += count;
                    if (count == 0)
                    {
                        throw new RemoteCloseException();
                    }
                    reportProgress?.Invoke(alreadyCount, length);
                }
                while (alreadyCount < length);
                return OperateResult.CreateSuccessResult(length);
            }
            count = await ssl.ReadAsync(buffer, offset, buffer.Length - offset).ConfigureAwait(false);
            if (count == 0)
            {
                throw new RemoteCloseException();
            }
            return OperateResult.CreateSuccessResult(count);
        }
        catch (RemoteCloseException)
        {
            ssl?.Close();
            return new OperateResult<int>(SocketErrorCode, StringResources.Language.RemoteClosedConnection);
        }
        catch (Exception ex)
        {
            ssl?.Close();
            return new OperateResult<int>(SocketErrorCode, "Socket Exception -> " + ex.Message);
        }
    }

    /// <inheritdoc />
    public static OperateResult SocketSend(Socket socket, byte[] data)
    {
        if (data == null)
        {
            return OperateResult.CreateSuccessResult();
        }
        return SocketSend(socket, data, 0, data.Length);
    }

    /// <summary>
    /// 发送消息给套接字，直到完成的时候返回，经过测试，本方法是线程安全的。
    /// </summary>
    /// <param name="socket">网络套接字</param>
    /// <param name="data">字节数据</param>
    /// <param name="offset">偏移的位置信息</param>
    /// <param name="size">发送的数据总数</param>
    /// <returns>发送是否成功的结果</returns>
    public static OperateResult SocketSend(Socket socket, byte[] data, int offset, int size)
    {
        if (data == null)
        {
            return OperateResult.CreateSuccessResult();
        }

        if (socket == null)
        {
            return new OperateResult<byte[]>(SocketErrorCode, "Socket is null");
        }
        try
        {
            var num = 0;
            do
            {
                var num2 = socket.Send(data, offset, size - num, SocketFlags.None);
                num += num2;
                offset += num2;
            }
            while (num < size && num < data.Length);
            return OperateResult.CreateSuccessResult();
        }
        catch (Exception ex)
        {
            socket?.Close();
            return new OperateResult<byte[]>(SocketErrorCode, ex.Message);
        }
    }

    /// <summary>
    /// 发送消息给套接字，直到完成的时候返回，经过测试，本方法是线程安全的。
    /// </summary>
    /// <param name="socket">网络套接字</param>
    /// <param name="data">字节数据</param>
    /// <returns>发送是否成功的结果</returns>
    public static async Task<OperateResult> SocketSendAsync(Socket socket, byte[] data)
    {
        if (data == null)
        {
            return await Task.FromResult(OperateResult.CreateSuccessResult()).ConfigureAwait(false);
        }
        return await SocketSendAsync(socket, data, 0, data.Length).ConfigureAwait(false);
    }

    /// <summary>
    /// 发送消息给套接字，直到完成的时候返回，经过测试，本方法是线程安全的。
    /// </summary>
    /// <param name="socket">网络套接字</param>
    /// <param name="data">字节数据</param>
    /// <param name="offset">偏移的位置信息</param>
    /// <param name="size">发送的数据总数</param>
    /// <returns>发送是否成功的结果</returns>
    public static async Task<OperateResult> SocketSendAsync(Socket socket, byte[] data, int offset, int size)
    {
        if (data == null)
        {
            return OperateResult.CreateSuccessResult();
        }

        if (socket == null)
        {
            return new OperateResult<byte[]>(SocketErrorCode, "Socket is null");
        }

        var sendCount = 0;
        try
        {
            do
            {
                var count = await Task.Factory.FromAsync(socket.BeginSend(data, offset, size - sendCount, SocketFlags.None, null, socket), socket.EndSend).ConfigureAwait(false);
                sendCount += count;
                offset += count;
            }
            while (sendCount < size && sendCount < data.Length);
            return OperateResult.CreateSuccessResult();
        }
        catch (Exception ex)
        {
            socket?.Close();
            return new OperateResult<byte[]>(SocketErrorCode, ex.Message);
        }
    }

    /// <inheritdoc />
    public static OperateResult SocketSend(SslStream ssl, byte[] data)
    {
        if (data == null)
        {
            return OperateResult.CreateSuccessResult();
        }
        return SocketSend(ssl, data, 0, data.Length);
    }

    /// <inheritdoc />
    public static OperateResult SocketSend(SslStream ssl, byte[] data, int offset, int size)
    {
        if (data == null)
        {
            return OperateResult.CreateSuccessResult();
        }

        if (ssl == null)
        {
            return new OperateResult(SocketErrorCode, "SslStream is null");
        }
        try
        {
            ssl.Write(data, offset, size);
            return OperateResult.CreateSuccessResult();
        }
        catch (Exception ex)
        {
            ssl?.Close();
            return new OperateResult<byte[]>(SocketErrorCode, ex.Message);
        }
    }

    /// <inheritdoc />
    public static async Task<OperateResult> SocketSendAsync(SslStream ssl, byte[] data, int offset, int size)
    {
        if (data == null)
        {
            return OperateResult.CreateSuccessResult();
        }

        if (ssl == null)
        {
            return new OperateResult(SocketErrorCode, "SslStream is null");
        }
        try
        {
            await ssl.WriteAsync(data.AsMemory(offset, size)).ConfigureAwait(false);
            return OperateResult.CreateSuccessResult();
        }
        catch (Exception ex)
        {
            ssl?.Close();
            return new OperateResult<byte[]>(SocketErrorCode, ex.Message);
        }
    }

    /// <inheritdoc />
    public static async Task<OperateResult<int>> ReadStreamAsync(Stream stream, byte[] buffer)
    {
        try
        {
            return OperateResult.CreateSuccessResult(await stream.ReadAsync(buffer).ConfigureAwait(false));
        }
        catch (Exception ex)
        {
            stream?.Close();
            return new OperateResult<int>(ex.Message);
        }
    }

    /// <inheritdoc />
    public static async Task<OperateResult> WriteStreamAsync(Stream stream, byte[] buffer)
    {
        var alreadyCount = 0;
        try
        {
            await stream.WriteAsync(buffer.AsMemory(alreadyCount, buffer.Length - alreadyCount)).ConfigureAwait(false);
            return OperateResult.CreateSuccessResult(alreadyCount);
        }
        catch (Exception ex)
        {
            stream?.Close();
            return new OperateResult<int>(ex.Message);
        }
    }
}
