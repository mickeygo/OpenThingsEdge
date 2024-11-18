using System.Net.NetworkInformation;
using System.Net.Sockets;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Core.Pipe;

namespace ThingsEdge.Communication.Core.Net;

/// <summary>
/// 支持长连接，短连接两个模式的通用客户端基类。
/// </summary>
public class NetworkDoubleBase : NetworkBase, IDisposable
{
    /// <summary>
    /// 当前的网络的管道信息
    /// </summary>
    protected PipeSocket pipeSocket;
    private readonly bool _isUseSpecifiedSocket;

    private bool _useServerActivePush;

    private AutoResetEvent _autoResetEvent;

    private byte[] _bufferQA = null;

    /// <summary>
    /// 是否是长连接的状态。
    /// </summary>
    private bool _isPersistentConn;

    private bool _disposedValue;

    private byte[] _sendbyteBefore = null;

    private string _sendBefore = string.Empty;

    private readonly Lazy<Ping> _ping = new(() => new Ping());

    /// <summary>
    /// 获取或设置当前的连接是否激活从服务器主动推送的功能
    /// </summary>
    protected bool UseServerActivePush
    {
        get
        {
            return _useServerActivePush;
        }
        set
        {
            if (value)
            {
                if (_autoResetEvent == null)
                {
                    _autoResetEvent = new AutoResetEvent(initialState: false);
                }
                _isPersistentConn = true;
            }
            _useServerActivePush = value;
        }
    }

    public IByteTransform ByteTransform { get; set; }

    /// <summary>
    /// 获取或设置连接的超时时间，单位是毫秒。
    /// </summary>
    public virtual int ConnectTimeOut
    {
        get
        {
            return pipeSocket.ConnectTimeOut;
        }
        set
        {
            if (value >= 0)
            {
                pipeSocket.ConnectTimeOut = value;
            }
        }
    }

    /// <summary>
    /// 获取或设置接收服务器反馈的时间，如果为负数，则不接收反馈。
    /// </summary>
    public int ReceiveTimeOut
    {
        get
        {
            return pipeSocket.ReceiveTimeOut;
        }
        set
        {
            pipeSocket.ReceiveTimeOut = value;
        }
    }

    /// <summary>
    /// 获取或是设置远程服务器的IP地址，如果是本机测试，那么需要设置为127.0.0.1。
    /// </summary>
    /// <remarks>
    /// 最好实在初始化的时候进行指定，当使用短连接的时候，支持动态更改，切换；当使用长连接后，无法动态更改。
    /// </remarks>
    public virtual string IpAddress
    {
        get
        {
            return pipeSocket.IpAddress;
        }
        set
        {
            pipeSocket.IpAddress = value;
        }
    }

    /// <summary>
    /// 获取或设置服务器的端口号，具体的值需要取决于对方的配置。
    /// </summary>
    /// <remarks>
    /// 最好实在初始化的时候进行指定，当使用短连接的时候，支持动态更改，切换；当使用长连接后，无法动态更改
    /// </remarks>
    public virtual int Port
    {
        get
        {
            return pipeSocket.Port;
        }
        set
        {
            pipeSocket.Port = value;
        }
    }

    /// <inheritdoc cref="IReadWriteNet.ConnectionId" />
    public string ConnectionId { get; init; }

    /// <summary>
    /// 获取或设置在正式接收对方返回数据前的时候，需要休息的时间，当设置为0的时候，不需要休息。
    /// </summary>
    public int SleepTime
    {
        get
        {
            return pipeSocket.SleepTime;
        }
        set
        {
            pipeSocket.SleepTime = value;
        }
    }

    /// <summary>
    /// 当前的异形连接对象，如果设置了异形连接的话，仅用于异形模式的情况使用。
    /// </summary>
    /// <remarks>
    /// 具体的使用方法请参照Demo项目中的异形modbus实现。
    /// </remarks>
    public AlienSession AlienSession { get; set; }

    /// <inheritdoc cref="SocketKeepAliveTime" />
    public int SocketKeepAliveTime { get; set; } = -1;

    /// <summary>
    /// 默认的无参构造函数。
    /// </summary>
    public NetworkDoubleBase()
    {
        pipeSocket = new PipeSocket();
        ConnectionId = SoftBasic.GetUniqueStringByGuidAndRandom();
    }

    /// <summary>
    /// 获取一个新的消息对象的方法，需要在继承类里面进行重写。
    /// </summary>
    /// <returns>消息类对象</returns>
    protected virtual INetMessage GetNewNetMessage()
    {
        return null;
    }

    /// <summary>
    /// 在读取数据之前可以调用本方法将客户端设置为长连接模式，相当于跳过了ConnectServer的结果验证，对异形客户端无效，当第一次进行通信时再进行创建连接请求。
    /// </summary>
    public void SetPersistentConnection()
    {
        _isPersistentConn = true;
    }

    /// <summary>
    /// 对当前设备的IP地址进行PING的操作，返回PING的结果 />。
    /// <returns>返回PING的结果</returns>
    public IPStatus IpAddressPing()
    {
        return _ping.Value.Send(IpAddress).Status;
    }

    private async void ServerSocketActivePushAsync(IAsyncResult ar)
    {
        var asyncState = ar.AsyncState;
        if (asyncState is not Socket socket)
        {
            return;
        }

        var endResult = socket.EndReceiveResult(ar);
        if (!endResult.IsSuccess)
        {
            pipeSocket.IsSocketError = true;
            return;
        }

        var receive = await base.ReceiveByMessageAsync(socket, ReceiveTimeOut, GetNewNetMessage()).ConfigureAwait(false);
        if (!receive.IsSuccess)
        {
            pipeSocket.IsSocketError = true;
            return;
        }

        var receiveAgain = socket.BeginReceiveResult(ServerSocketActivePushAsync);
        if (!receiveAgain.IsSuccess)
        {
            pipeSocket.IsSocketError = true;
        }
        if (DecideWhetherQAMessage(socket, receive))
        {
            _bufferQA = receive.Content;
            _autoResetEvent.Set();
        }
    }

    /// <summary>
    /// 决定当前的消息是否是应答机制的消息内容，需要在客户端进行重写实现，如果是应答机制，返回 <c>True</c>, 否则返回 <c>False</c>。
    /// </summary>
    /// <param name="socket">通信使用的网络套接字</param>
    /// <param name="receive">服务器返回的内容</param>
    /// <returns>是否应答机制的数据报文</returns>
    protected virtual bool DecideWhetherQAMessage(Socket socket, OperateResult<byte[]> receive)
    {
        return true;
    }

    /// <summary>
    /// 和服务器交互完成的时候调用的方法，可以根据读写结果进行一些额外的操作，具体的操作需要根据实际的需求来重写实现。
    /// </summary>
    /// <param name="read">读取结果</param>
    protected virtual void ExtraAfterReadFromCoreServer(OperateResult read)
    {
    }

    /// <summary>
    /// 根据实际的协议选择是否重写本方法，有些协议在创建连接之后，需要进行一些初始化的信号握手，才能最终建立网络通道。
    /// </summary>
    /// <param name="socket">网络套接字</param>
    /// <returns>是否初始化成功，依据具体的协议进行重写</returns>
    protected virtual async Task<OperateResult> InitializationOnConnectAsync(Socket socket)
    {
        if (_useServerActivePush)
        {
            var receive = socket.BeginReceiveResult(ServerSocketActivePushAsync);
            if (!receive.IsSuccess)
            {
                return receive;
            }
        }
        return await Task.FromResult(OperateResult.CreateSuccessResult()).ConfigureAwait(false);
    }

    /// <summary>
    /// 根据实际的协议选择是否重写本方法，有些协议在断开连接之前，需要发送一些报文来关闭当前的网络通道。
    /// </summary>
    /// <param name="socket">网络套接字</param>
    protected virtual Task<OperateResult> ExtraOnDisconnectAsync(Socket socket)
    {
        return Task.FromResult(OperateResult.CreateSuccessResult());
    }

    /// <summary>
    /// 尝试连接服务器，如果成功，并执行<see cref="InitializationOnConnect" />的初始化方法，并返回最终的结果。
    /// </summary>
    /// <returns>带有socket的结果对象</returns>
    private async Task<OperateResult<Socket>> CreateSocketAndInitialicationAsync()
    {
        var operateResult = await CreateSocketAndConnectAsync(pipeSocket.GetConnectIPEndPoint(), ConnectTimeOut).ConfigureAwait(continueOnCapturedContext: false);
        var result = operateResult;
        if (result.IsSuccess)
        {
            var initi = await InitializationOnConnectAsync(result.Content).ConfigureAwait(continueOnCapturedContext: false);
            if (!initi.IsSuccess)
            {
                result.Content?.Close();
                result.IsSuccess = initi.IsSuccess;
                result.CopyErrorFromOther(initi);
            }
        }
        return result;
    }

    /// <inheritdoc cref="M:HslCommunication.Core.Net.NetworkDoubleBase.GetAvailableSocket" />
    protected async Task<OperateResult<Socket>> GetAvailableSocketAsync()
    {
        if (_isPersistentConn)
        {
            if (_isUseSpecifiedSocket)
            {
                if (pipeSocket.IsSocketError)
                {
                    return new OperateResult<Socket>(StringResources.Language.ConnectionIsNotAvailable);
                }
                return OperateResult.CreateSuccessResult(pipeSocket.Socket);
            }
            if (pipeSocket.IsConnectitonError())
            {
                var connect = await ConnectServerAsync().ConfigureAwait(continueOnCapturedContext: false);
                if (!connect.IsSuccess)
                {
                    pipeSocket.IsSocketError = true;
                    return OperateResult.CreateFailedResult<Socket>(connect);
                }
                pipeSocket.IsSocketError = false;
                return OperateResult.CreateSuccessResult(pipeSocket.Socket);
            }
            return OperateResult.CreateSuccessResult(pipeSocket.Socket);
        }
        return await CreateSocketAndInitialicationAsync().ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <summary>
    /// 尝试连接远程的服务器，如果连接成功，就切换短连接模式到长连接模式，后面的每次请求都共享一个通道，使得通讯速度更快速。
    /// </summary>
    /// <returns>返回连接结果，如果失败的话（也即IsSuccess为False），包含失败信息</returns>
    public async Task<OperateResult> ConnectServerAsync()
    {
        _isPersistentConn = true;
        pipeSocket.Socket?.Close();
        var rSocket = await CreateSocketAndInitialicationAsync().ConfigureAwait(continueOnCapturedContext: false);
        if (!rSocket.IsSuccess)
        {
            pipeSocket.IsSocketError = true;
            rSocket.Content = null;
            return rSocket;
        }
        pipeSocket.Socket = rSocket.Content;
        if (SocketKeepAliveTime > 0)
        {
            rSocket.Content.SetKeepAlive(SocketKeepAliveTime, SocketKeepAliveTime);
        }

        return rSocket;
    }

    /// <summary>
    /// 手动断开与远程服务器的连接，如果当前是长连接模式，那么就会切换到短连接模式。
    /// </summary>
    /// <returns>关闭连接，不需要查看IsSuccess属性查看</returns>
    public async Task<OperateResult> ConnectCloseAsync()
    {
        new OperateResult();
        _isPersistentConn = false;
        OperateResult result;
        try
        {
            result = await ExtraOnDisconnectAsync(pipeSocket.Socket).ConfigureAwait(continueOnCapturedContext: false);
            pipeSocket.Socket?.Close();
            pipeSocket.Socket = null;
        }
        catch
        {
            throw;
        }

        return result;
    }

    /// <inheritdoc cref="M:HslCommunication.Core.Net.NetworkDoubleBase.ReadFromCoreServer(System.Net.Sockets.Socket,System.Byte[],System.Boolean,System.Boolean)" />
    public virtual async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(Socket socket, byte[] send, bool hasResponseData = true, bool usePackAndUnpack = true)
    {
        var sendValue = usePackAndUnpack ? PackCommandWithHeader(send) : send;
        var netMessage = GetNewNetMessage();
        if (netMessage != null)
        {
            netMessage.SendBytes = sendValue;
        }
        if (_sendbyteBefore != null)
        {
            await SendAsync(socket, _sendbyteBefore).ConfigureAwait(continueOnCapturedContext: false);
        }
        OperateResult operateResult = await SendAsync(socket, sendValue).ConfigureAwait(continueOnCapturedContext: false);
        var sendResult = operateResult;
        if (!sendResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(sendResult);
        }
        if (ReceiveTimeOut < 0)
        {
            return OperateResult.CreateSuccessResult(new byte[0]);
        }
        if (!hasResponseData)
        {
            return OperateResult.CreateSuccessResult(new byte[0]);
        }
        if (SleepTime > 0)
        {
            CommHelper.ThreadSleep(SleepTime);
        }
        OperateResult<byte[]> resultReceive;
        if (_useServerActivePush)
        {
            if (!await Task.Run(() => _autoResetEvent.WaitOne(ReceiveTimeOut)).ConfigureAwait(continueOnCapturedContext: false))
            {
                NetSupport.CloseSocket(socket);
                pipeSocket.IsSocketError = true;
                return new OperateResult<byte[]>(-10000, StringResources.Language.ReceiveDataTimeout + ReceiveTimeOut);
            }
            netMessage.HeadBytes = _bufferQA;
            resultReceive = OperateResult.CreateSuccessResult(_bufferQA);
        }
        else if (netMessage == null)
        {
            var startTime = DateTime.Now;
            var ms = new MemoryStream();
            while (true)
            {
                var read = await ReceiveByMessageAsync(socket, ReceiveTimeOut, netMessage).ConfigureAwait(continueOnCapturedContext: false);
                if (!read.IsSuccess)
                {
                    return read;
                }
                ms.Write(read.Content);
                if (CheckReceiveDataComplete(sendValue, ms))
                {
                    resultReceive = OperateResult.CreateSuccessResult(ms.ToArray());
                    break;
                }
                if (ReceiveTimeOut > 0 && (DateTime.Now - startTime).TotalMilliseconds > ReceiveTimeOut)
                {
                    resultReceive = new OperateResult<byte[]>(StringResources.Language.ReceiveDataTimeout + ReceiveTimeOut);
                    break;
                }
            }
        }
        else
        {
            resultReceive = await ReceiveByMessageAsync(socket, ReceiveTimeOut, netMessage).ConfigureAwait(continueOnCapturedContext: false);
        }
        if (!resultReceive.IsSuccess)
        {
            return resultReceive;
        }

        if (netMessage != null && !netMessage.CheckHeadBytesLegal(Token.ToByteArray()))
        {
            NetSupport.CloseSocket(socket);
            return new OperateResult<byte[]>(StringResources.Language.CommandHeadCodeCheckFailed + Environment.NewLine + StringResources.Language.Send + ": " + SoftBasic.ByteToHexString(sendValue, ' ') + Environment.NewLine + StringResources.Language.Receive + ": " + SoftBasic.ByteToHexString(resultReceive.Content, ' '));
        }
        return usePackAndUnpack ? UnpackResponseContent(sendValue, resultReceive.Content) : resultReceive;
    }

    /// <inheritdoc cref="M:HslCommunication.Core.Net.NetworkDoubleBase.ReadFromCoreServer(System.Byte[],System.Boolean,System.Boolean)" />
    public virtual async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(byte[] send)
    {
        return await ReadFromCoreServerAsync(send, hasResponseData: true).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <inheritdoc cref="M:HslCommunication.Core.Net.NetworkDoubleBase.ReadFromCoreServer(System.Collections.Generic.IEnumerable{System.Byte[]})" />
    public async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(IEnumerable<byte[]> send)
    {
        return await NetSupport.ReadFromCoreServerAsync(send, ReadFromCoreServerAsync).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <summary>
    /// 将数据报文发送指定的网络通道上，根据当前指定的<see cref="INetMessage" />类型，返回一条完整的数据指令。
    /// </summary>
    /// <param name="send">发送的完整的报文信息</param>
    /// <param name="hasResponseData">是否有等待的数据返回，默认为 true</param>
    /// <param name="usePackAndUnpack">是否需要对命令重新打包，在重写<see cref="PackCommandWithHeader(byte[])" />方法后才会有影响</param>
    /// <remarks>
    /// 无锁的基于套接字直接进行叠加协议的操作。
    /// </remarks>
    /// <returns>接收的完整的报文信息</returns>
    public async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(byte[] send, bool hasResponseData, bool usePackAndUnpack = true)
    {
        if (pipeSocket.LockingTick > CommHelper.LockLimit)
        {
            return new OperateResult<byte[]>(StringResources.Language.TooManyLock);
        }
        var result = new OperateResult<byte[]>();
        pipeSocket.PipeLockEnter();
        OperateResult<Socket> resultSocket;
        try
        {
            resultSocket = await GetAvailableSocketAsync().ConfigureAwait(continueOnCapturedContext: false);
            if (!resultSocket.IsSuccess)
            {
                pipeSocket.IsSocketError = true;
                AlienSession?.Offline();
                pipeSocket.PipeLockLeave();
                result.CopyErrorFromOther(resultSocket);
                return result;
            }
            var read = await ReadFromCoreServerAsync(resultSocket.Content, send, hasResponseData, usePackAndUnpack).ConfigureAwait(continueOnCapturedContext: false);
            if (read.IsSuccess)
            {
                pipeSocket.IsSocketError = false;
                result.IsSuccess = read.IsSuccess;
                result.Content = read.Content;
                result.Message = StringResources.Language.SuccessText;
            }
            else
            {
                if (read.ErrorCode != int.MinValue)
                {
                    pipeSocket.IsSocketError = true;
                    AlienSession?.Offline();
                }
                else
                {
                    read.ErrorCode = 10000;
                }
                result.CopyErrorFromOther(read);
            }
            ExtraAfterReadFromCoreServer(read);
            pipeSocket.PipeLockLeave();
        }
        catch
        {
            pipeSocket.PipeLockLeave();
            throw;
        }
        if (!_isPersistentConn)
        {
            resultSocket?.Content?.Close();
        }
        return result;
    }

    /// <summary>
    /// 检查当前从网口接收的数据是否是完整的，如果是完整的，则需要返回 <c>True</c>，表示数据接收立即完成，默认返回 <c>True</c>。
    /// </summary>
    /// <remarks>
    /// 在默认情况下，网口在接收数据之后，直接认为本次的数据接收已经完成，如果碰到有结束标记的协议，则可以重写本方法，然后加入额外的验证信息，直到全部数据接收完成。
    /// </remarks>
    /// <param name="send">当前发送的数据信息</param>
    /// <param name="ms">目前已经接收到数据流</param>
    /// <returns>如果数据接收完成，则返回True, 否则返回False</returns>
    protected virtual bool CheckReceiveDataComplete(byte[] send, MemoryStream ms)
    {
        return true;
    }

    /// <summary>
    /// 对当前的命令进行打包处理，通常是携带命令头内容，标记当前的命令的长度信息，需要进行重写，否则默认不打包。
    /// </summary>
    /// <remarks>
    /// 对发送的命令打包之后，直接发送给真实的对方设备了，例如在AB-PLC里面，就重写了打包方法，将当前的会话ID参数传递给PLC设备。
    /// </remarks>
    /// <param name="command">发送的数据命令内容</param>
    /// <returns>打包之后的数据结果信息</returns>
    public virtual byte[] PackCommandWithHeader(byte[] command)
    {
        return command;
    }

    /// <summary>
    /// 根据对方返回的报文命令，对命令进行基本的拆包，例如各种Modbus协议拆包为统一的核心报文，还支持对报文的验证。
    /// </summary>
    /// <remarks>
    /// 在实际解包的操作过程中，通常对状态码，错误码等消息进行判断，如果校验不通过，将携带错误消息返回。
    /// </remarks>
    /// <param name="send">发送的原始报文数据</param>
    /// <param name="response">设备方反馈的原始报文内容</param>
    /// <returns>返回拆包之后的报文信息，默认不进行任何的拆包操作</returns>
    public virtual OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        return OperateResult.CreateSuccessResult(response);
    }

    /// <summary>
    /// 释放当前的资源，并自动关闭长连接，如果设置了的话
    /// </summary>
    /// <param name="disposing">是否释放托管的资源信息</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                ConnectClose();
            }
            _disposedValue = true;
        }
    }

    /// <summary>
    /// 释放当前的资源，如果调用了本方法，那么该对象再使用的时候，需要重新实例化。
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var newNetMessage = GetNewNetMessage();
        var text2 = ByteTransform == null ? "IByteTransform" : ByteTransform.GetType().ToString();
        return $"NetworkDoubleBase<{newNetMessage}, {text2}>[{IpAddress}:{Port}]";
    }
}
