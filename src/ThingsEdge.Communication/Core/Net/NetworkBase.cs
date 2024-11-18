using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Exceptions;

namespace ThingsEdge.Communication.Core.Net;

/// <summary>
/// 本系统所有网络类的基类，该类为抽象类，无法进行实例化，如果想使用里面的方法来实现自定义的网络通信，请通过继承使用。
/// </summary>
public abstract class NetworkBase
{
    private int _connectErrorCount;

    /// <summary>
    /// 组件的日志工具。
    /// </summary>
    /// <remarks>
    public ILogger? Logger { get; set; }

    /// <summary>
    /// 网络类的身份令牌，在hsl协议的模式下会有效，在和设备进行通信的时候是无效的。
    /// </summary>
    /// <remarks>
    /// 适用于Hsl协议相关的网络通信类，不适用于设备交互类。
    /// </remarks>
    public Guid Token { get; } = Guid.Empty;

    /// <summary>
    /// 实例化一个NetworkBase对象，令牌的默认值为空，都是0x00。
    /// </summary>
    public NetworkBase()
    {
        
    }


    /// <summary>
    /// 创建一个新的socket对象并连接到远程的地址，需要指定远程终结点，超时时间（单位是毫秒）。
    /// </summary>
    /// <param name="endPoint">连接的目标终结点</param>
    /// <param name="timeOut">连接的超时时间</param>
    /// <returns>返回套接字的封装结果对象</returns>
    protected async Task<OperateResult<Socket>> CreateSocketAndConnectAsync(IPEndPoint endPoint, int timeOut)
    {
        var connect = await NetSupport.CreateSocketAndConnectAsync(endPoint, timeOut).ConfigureAwait(false);
        if (connect.IsSuccess)
        {
            _connectErrorCount = 0;
            return connect;
        }
        if (_connectErrorCount < 1000000000)
        {
            _connectErrorCount++;
        }
        return new OperateResult<Socket>(-_connectErrorCount, connect.Message);
    }

    /// <summary>
    /// 创建一个新的socket对象并连接到远程的地址，默认超时时间为10秒钟，需要指定ip地址以及端口号信息。
    /// </summary>
    /// <param name="ipAddress">Ip地址</param>
    /// <param name="port">端口号</param>
    /// <returns>返回套接字的封装结果对象</returns>
    /// <example>
    protected async Task<OperateResult<Socket>> CreateSocketAndConnectAsync(string ipAddress, int port)
    {
        return await CreateSocketAndConnectAsync(new IPEndPoint(IPAddress.Parse(ipAddress), port), 10000).ConfigureAwait(false);
    }

    /// <summary>
    /// 创建一个新的socket对象并连接到远程的地址，需要指定ip地址以及端口号信息，还有超时时间，单位是毫秒。
    /// </summary>
    /// <param name="ipAddress">Ip地址</param>
    /// <param name="port">端口号</param>
    /// <param name="timeOut">连接的超时时间</param>
    /// <returns>返回套接字的封装结果对象</returns>
    protected async Task<OperateResult<Socket>> CreateSocketAndConnectAsync(string ipAddress, int port, int timeOut)
    {
        return await CreateSocketAndConnectAsync(new IPEndPoint(IPAddress.Parse(ipAddress), port), timeOut).ConfigureAwait(continueOnCapturedContext: false);
    }

    protected OperateResult Send(SslStream ssl, byte[] data)
    {
        if (data == null)
        {
            return OperateResult.CreateSuccessResult();
        }
        return Send(ssl, data, 0, data.Length);
    }

    protected OperateResult Send(SslStream ssl, byte[] data, int offset, int size)
    {
        if (data == null)
        {
            return OperateResult.CreateSuccessResult();
        }

        try
        {
            ssl.Write(data, offset, size);
            return OperateResult.CreateSuccessResult();
        }
        catch (Exception ex)
        {
            ssl?.Close();
            if (_connectErrorCount < 1000000000)
            {
                _connectErrorCount++;
            }
            return new OperateResult<byte[]>(-_connectErrorCount, ex.Message);
        }
    }

    /// <summary>
    /// 接收固定长度的字节数组，允许指定超时时间，默认为60秒，当length大于0时，接收固定长度的数据内容，当length小于0时，接收不大于2048长度的随机数据信息。
    /// </summary>
    /// <param name="socket">网络通讯的套接字<br />Network communication socket</param>
    /// <param name="length">准备接收的数据长度，当length大于0时，接收固定长度的数据内容，当length小于0时，接收不大于1024长度的随机数据信息</param>
    /// <param name="timeOut">单位：毫秒，超时时间，默认为60秒，如果设置小于0，则不检查超时时间</param>
    /// <returns>包含了字节数据的结果类</returns>
    protected async Task<OperateResult<byte[]>> ReceiveAsync(Socket socket, int length, int timeOut = 60000)
    {
        if (length == 0)
        {
            return OperateResult.CreateSuccessResult(Array.Empty<byte>());
        }

        var bufferLength = length > 0 ? length : 2048;
        byte[] buffer;
        try
        {
            buffer = new byte[bufferLength];
        }
        catch (Exception ex)
        {
            socket?.Close();
            return new OperateResult<byte[]>($"Create byte[{bufferLength}] buffer failed: " + ex.Message);
        }

        var receive = await ReceiveAsync(socket, buffer, 0, length, timeOut).ConfigureAwait(false);
        if (!receive.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(receive);
        }
        return OperateResult.CreateSuccessResult(length > 0 ? buffer : buffer.SelectBegin(receive.Content));
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
    protected async Task<OperateResult<int>> ReceiveAsync(Socket socket, byte[] buffer, int offset, int length, int timeOut = 60000)
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
                return OperateResult.CreateSuccessResult(length);
            }
            count = await Task.Factory.FromAsync(socket.BeginReceive(buffer, offset, buffer.Length - offset, SocketFlags.None, null, socket), socket.EndReceive).ConfigureAwait(continueOnCapturedContext: false);
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
            if (_connectErrorCount < 1000000000)
            {
                _connectErrorCount++;
            }
            hslTimeOut.IsSuccessful = true;
            return new OperateResult<int>(-_connectErrorCount, StringResources.Language.RemoteClosedConnection);
        }
        catch (Exception ex)
        {
            socket?.Close();
            hslTimeOut.IsSuccessful = true;
            if (_connectErrorCount < 1000000000)
            {
                _connectErrorCount++;
            }
            if (hslTimeOut.IsTimeout)
            {
                return new OperateResult<int>(-_connectErrorCount, StringResources.Language.ReceiveDataTimeout + hslTimeOut.DelayTime);
            }
            return new OperateResult<int>(-_connectErrorCount, "Socket Exception -> " + ex.Message);
        }
    }

    /// <summary>
    /// 接收一行命令数据，需要自己指定这个结束符，默认超时时间为60秒，也即是60000，单位是毫秒。
    /// </summary>
    /// <param name="socket">网络套接字</param>
    /// <param name="endCode">结束符信息</param>
    /// <param name="timeout">超时时间，默认为60000，单位为毫秒，也就是60秒</param>
    /// <returns>带有结果对象的数据信息</returns>
    protected async Task<OperateResult<byte[]>> ReceiveCommandLineFromSocketAsync(Socket socket, byte endCode, int timeout = int.MaxValue)
    {
        var bufferArray = new List<byte>(128);
        try
        {
            var st = DateTime.Now;
            var bOK = false;
            while ((DateTime.Now - st).TotalMilliseconds < timeout)
            {
                if (socket.Poll(timeout, SelectMode.SelectRead))
                {
                    var headResult = await ReceiveAsync(socket, 1, timeout).ConfigureAwait(continueOnCapturedContext: false);
                    if (!headResult.IsSuccess)
                    {
                        return headResult;
                    }
                    bufferArray.AddRange(headResult.Content);
                    if (headResult.Content[0] == endCode)
                    {
                        bOK = true;
                        break;
                    }
                }
            }
            if (!bOK)
            {
                return new OperateResult<byte[]>(StringResources.Language.ReceiveDataTimeout);
            }
            return OperateResult.CreateSuccessResult(bufferArray.ToArray());
        }
        catch (Exception ex2)
        {
            var ex = ex2;
            socket?.Close();
            return new OperateResult<byte[]>(ex.Message);
        }
    }

    /// <summary>
    /// 接收一行命令数据，需要自己指定这个结束符，默认超时时间为60秒，也即是60000，单位是毫秒。
    /// </summary>
    /// <param name="socket">网络套接字</param>
    /// <param name="endCode1">结束符1信息</param>
    /// <param name="endCode2">结束符2信息</param>
    /// /// <param name="timeout">超时时间，默认无穷大，单位毫秒</param>
    /// <returns>带有结果对象的数据信息</returns>
    protected async Task<OperateResult<byte[]>> ReceiveCommandLineFromSocketAsync(Socket socket, byte endCode1, byte endCode2, int timeout = 60000)
    {
        var bufferArray = new List<byte>(128);
        try
        {
            var st = DateTime.Now;
            var bOK = false;
            while ((DateTime.Now - st).TotalMilliseconds < timeout)
            {
                if (socket.Poll(timeout, SelectMode.SelectRead))
                {
                    var headResult = await ReceiveAsync(socket, 1, timeout).ConfigureAwait(continueOnCapturedContext: false);
                    if (!headResult.IsSuccess)
                    {
                        return headResult;
                    }
                    bufferArray.AddRange(headResult.Content);
                    if (headResult.Content[0] == endCode2 && bufferArray.Count > 1 && bufferArray[bufferArray.Count - 2] == endCode1)
                    {
                        bOK = true;
                        break;
                    }
                }
            }
            if (!bOK)
            {
                return new OperateResult<byte[]>(StringResources.Language.ReceiveDataTimeout);
            }
            return OperateResult.CreateSuccessResult(bufferArray.ToArray());
        }
        catch (Exception ex2)
        {
            var ex = ex2;
            socket?.Close();
            return new OperateResult<byte[]>(ex.Message);
        }
    }

    /// <summary>
    /// 发送消息给套接字，直到完成的时候返回，经过测试，本方法是线程安全的。
    /// </summary>
    /// <param name="socket">网络套接字</param>
    /// <param name="data">字节数据</param>
    /// <returns>发送是否成功的结果</returns>
    protected async Task<OperateResult> SendAsync(Socket socket, byte[] data)
    {
        if (data == null)
        {
            return OperateResult.CreateSuccessResult();
        }
        return await SendAsync(socket, data, 0, data.Length).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <summary>
    /// 发送消息给套接字，直到完成的时候返回，经过测试，本方法是线程安全的。
    /// </summary>
    /// <param name="socket">网络套接字</param>
    /// <param name="data">字节数据</param>
    /// <param name="offset">偏移的位置信息</param>
    /// <param name="size">发送的数据总数</param>
    /// <returns>发送是否成功的结果</returns>
    protected async Task<OperateResult> SendAsync(Socket socket, byte[] data, int offset, int size)
    {
        if (data == null)
        {
            return OperateResult.CreateSuccessResult();
        }

        var alreadyCount = 0;
        try
        {
            do
            {
                var count = await Task.Factory.FromAsync(socket.BeginSend(data, offset, size - alreadyCount, SocketFlags.None, null, socket), socket.EndSend).ConfigureAwait(continueOnCapturedContext: false);
                alreadyCount += count;
                offset += count;
            }
            while (alreadyCount < size);
            return OperateResult.CreateSuccessResult();
        }
        catch (Exception ex)
        {
            socket?.Close();
            if (_connectErrorCount < 1000000000)
            {
                _connectErrorCount++;
            }
            return new OperateResult<byte[]>(-_connectErrorCount, ex.Message);
        }
    }

    /// <summary>
    /// 接收一条完整的 <seealso cref="INetMessage" /> 数据内容，需要指定超时时间，单位为毫秒。
    /// </summary>
    /// <param name="socket">网络的套接字</param>
    /// <param name="timeOut">超时时间，单位：毫秒</param>
    /// <param name="netMessage">消息的格式定义</param>
    /// <returns>带有是否成功的byte数组对象</returns> />
    protected virtual async Task<OperateResult<byte[]>> ReceiveByMessageAsync(Socket socket, int timeOut, INetMessage netMessage)
    {
        if (netMessage == null)
        {
            return await ReceiveAsync(socket, -1, timeOut).ConfigureAwait(continueOnCapturedContext: false);
        }

        if (netMessage.ProtocolHeadBytesLength < 0)
        {
            var headCode = BitConverter.GetBytes(netMessage.ProtocolHeadBytesLength);
            var codeLength = headCode[3] & 0xF;
            OperateResult<byte[]> receive = null;
            switch (codeLength)
            {
                case 1:
                    receive = await ReceiveCommandLineFromSocketAsync(socket, headCode[1], timeOut).ConfigureAwait(continueOnCapturedContext: false);
                    break;
                case 2:
                    receive = await ReceiveCommandLineFromSocketAsync(socket, headCode[1], headCode[0], timeOut).ConfigureAwait(continueOnCapturedContext: false);
                    break;
            }
            if (receive == null)
            {
                return new OperateResult<byte[]>("Receive by specified code failed, length check failed");
            }
            if (!receive.IsSuccess)
            {
                return receive;
            }
            netMessage.HeadBytes = receive.Content;
            if (netMessage is SpecifiedCharacterMessage message)
            {
                if (message.EndLength == 0)
                {
                    return receive;
                }
                var endResult = await ReceiveAsync(socket, message.EndLength, timeOut).ConfigureAwait(continueOnCapturedContext: false);
                if (!endResult.IsSuccess)
                {
                    return endResult;
                }
                return OperateResult.CreateSuccessResult(SoftBasic.SpliceArray(receive.Content, endResult.Content));
            }
            return receive;
        }
        var headResult = await ReceiveAsync(socket, netMessage.ProtocolHeadBytesLength, timeOut).ConfigureAwait(continueOnCapturedContext: false);
        if (!headResult.IsSuccess)
        {
            return headResult;
        }
        var start = netMessage.PependedUselesByteLength(headResult.Content);
        var cycleCount = 0;
        while (start >= netMessage.ProtocolHeadBytesLength)
        {
            headResult = await ReceiveAsync(socket, netMessage.ProtocolHeadBytesLength, timeOut).ConfigureAwait(continueOnCapturedContext: false);
            if (!headResult.IsSuccess)
            {
                return headResult;
            }
            start = netMessage.PependedUselesByteLength(headResult.Content);
            cycleCount++;
            if (cycleCount > 10)
            {
                break;
            }
        }
        if (start > 0)
        {
            var head2Result = await ReceiveAsync(socket, start, timeOut).ConfigureAwait(continueOnCapturedContext: false);
            if (!head2Result.IsSuccess)
            {
                return head2Result;
            }
            headResult.Content = SoftBasic.SpliceArray(headResult.Content.RemoveBegin(start), head2Result.Content);
        }
        netMessage.HeadBytes = headResult.Content;
        var contentLength = netMessage.GetContentLengthByHeadBytes();
        if (contentLength <= 0)
        {
            return OperateResult.CreateSuccessResult(headResult.Content);
        }
        var buffer = new byte[netMessage.ProtocolHeadBytesLength + contentLength];
        headResult.Content.CopyTo(buffer, 0);
        var contentResult = await ReceiveAsync(socket, buffer, netMessage.ProtocolHeadBytesLength, contentLength, timeOut).ConfigureAwait(false);
        if (!contentResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(contentResult);
        }
        return OperateResult.CreateSuccessResult(buffer);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "NetworkBase";
    }
}
