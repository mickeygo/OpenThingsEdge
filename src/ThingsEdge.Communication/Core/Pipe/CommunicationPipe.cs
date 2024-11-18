using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core.IMessage;

namespace ThingsEdge.Communication.Core.Pipe;

/// <summary>
/// 通信的管道信息基类。
/// </summary>
public abstract class CommunicationPipe : IDisposable
{
    private bool _disposedValue;

    private int _receiveTimeOut = 5_000;
    private bool _useServerActivePush;

    private int _connectErrorCount;

    private ICommunicationLock _communicationLock;

    /// <summary>
    /// 当启用设备方主动发送数据时，用于同步访问方法的信号同步功能
    /// </summary>
    private AutoResetEvent _autoResetEvent;

    /// <summary>
    /// 当启用设备方主动发送数据时，用于应答服务机制的数据缓存
    /// </summary>
    protected byte[] BufferQA { get; set; }

    /// <summary>
    /// 获取或设置接收服务器反馈的时间，如果为负数，则不接收反馈。
    /// </summary>
    public int ReceiveTimeOut
    {
        get
        {
            return _receiveTimeOut;
        }
        set
        {
            _receiveTimeOut = value;
        }
    }

    /// <summary>
    /// 获取或设置在正式接收对方返回数据前的时候，需要休息的时间，当设置为0的时候，不需要休息。
    /// </summary>
    public int SleepTime { get; set; }

    /// <summary>
    /// 获取或设置当前的管道是否激活从设备主动推送的功能，设置为 true 时支持主动从设备方接收数据信息。
    /// </summary>
    public bool UseServerActivePush
    {
        get
        {
            return _useServerActivePush;
        }
        set
        {
            if (value)
            {
                _autoResetEvent ??= new AutoResetEvent(initialState: false);
            }
            _useServerActivePush = value;
        }
    }

    /// <summary>
    /// 获取或设置当前管道的线程锁对象，默认是简单的一个互斥锁。
    /// </summary>
    public ICommunicationLock CommunicationLock
    {
        get
        {
            return _communicationLock;
        }
        set
        {
            _communicationLock = value;
        }
    }

    /// <summary>
    /// 获取或设置当前的管道是否是长连接，仅对于串口及TCP是有效的，默认都是长连接。
    /// </summary>
    public bool IsPersistentConnection { get; set; } = true;

    /// <summary>
    /// 用来决定当前接收的消息是否是问答服务的消息。
    /// </summary>
    public Func<CommunicationPipe, OperateResult<byte[]>, bool>? DecideWhetherQAMessageFunction { get; set; }

    /// <summary>
    /// 实例化一个默认的构造对象
    /// </summary>
    public CommunicationPipe()
    {
        _communicationLock = new CommunicationLockSimple();
    }

    /// <summary>
    /// 接收固定长度的字节数组，允许指定超时时间，默认为60秒，当length大于0时，接收固定长度的数据内容，当length小于0时，buffer长度的缓存数据。
    /// </summary>
    /// <param name="buffer">等待接收的数据缓存信息</param>
    /// <param name="offset">开始接收数据的偏移地址</param>
    /// <param name="length">准备接收的数据长度，当length大于0时，接收固定长度的数据内容，当length小于0时，接收不大于2048长度的随机数据信息</param>
    /// <param name="timeOut">单位：毫秒，超时时间，默认为60秒，如果设置小于0，则不检查超时时间</param>
    /// <returns>包含了字节数据的结果类</returns>
    public virtual async Task<OperateResult<int>> ReceiveAsync(byte[] buffer, int offset, int length, int timeOut = 60000)
    {
        return await Task.FromResult(new OperateResult<int>(StringResources.Language.NotSupportedFunction)).ConfigureAwait(false);
    }

    public virtual async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(INetMessage netMessage, byte[] sendValue, bool hasResponseData)
    {
        var read = await ReadFromCoreServerHelperAsync(netMessage, sendValue, hasResponseData, SleepTime).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            if (read.ErrorCode < 0 && read.ErrorCode != int.MinValue)
            {
            }
            return read;
        }
        ResetConnectErrorCount();
        return read;
    }

    /// <summary>
    /// 根据给定的消息，发送的数据，接收到数据来判断是否接收完成报文
    /// </summary>
    /// <param name="netMessage">消息类对象</param>
    /// <param name="sendValue">发送的数据内容</param>
    /// <param name="ms">接收数据的流</param>
    /// <returns>是否接收完成数据</returns>
    protected bool CheckMessageComplete(INetMessage netMessage, byte[] sendValue, ref MemoryStream ms)
    {
        if (netMessage == null)
        {
            return true;
        }
        if (netMessage is SpecifiedCharacterMessage specifiedCharacterMessage)
        {
            var array = ms.ToArray();
            var bytes = BitConverter.GetBytes(specifiedCharacterMessage.ProtocolHeadBytesLength);
            switch (bytes[3] & 0xF)
            {
                case 1:
                    if (array.Length > specifiedCharacterMessage.EndLength
                        && array[array.Length - 1 - specifiedCharacterMessage.EndLength] == bytes[1])
                    {
                        return true;
                    }
                    break;
                case 2:
                    if (array.Length > specifiedCharacterMessage.EndLength + 1
                        && array[array.Length - 2 - specifiedCharacterMessage.EndLength] == bytes[1]
                        && array[array.Length - 1 - specifiedCharacterMessage.EndLength] == bytes[0])
                    {
                        return true;
                    }
                    break;
            }
        }
        else if (netMessage.ProtocolHeadBytesLength > 0)
        {
            var array2 = ms.ToArray();
            if (array2.Length >= netMessage.ProtocolHeadBytesLength)
            {
                var num = netMessage.PependedUselesByteLength(array2);
                if (num > 0)
                {
                    array2 = array2.RemoveBegin(num);
                    ms = new MemoryStream();
                    ms.Write(array2);
                    if (array2.Length < netMessage.ProtocolHeadBytesLength)
                    {
                        return false;
                    }
                }
                netMessage.HeadBytes = array2.SelectBegin(netMessage.ProtocolHeadBytesLength);
                netMessage.SendBytes = sendValue;
                var contentLengthByHeadBytes = netMessage.GetContentLengthByHeadBytes();
                if (array2.Length >= netMessage.ProtocolHeadBytesLength + contentLengthByHeadBytes)
                {
                    if (netMessage.ProtocolHeadBytesLength > netMessage.HeadBytes.Length)
                    {
                        ms = new MemoryStream();
                        ms.Write(array2.RemoveBegin(netMessage.ProtocolHeadBytesLength - netMessage.HeadBytes.Length));
                    }
                    return true;
                }
            }
        }
        else if (netMessage.CheckReceiveDataComplete(sendValue, ms))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 重置当前的连续错误计数为0，并且返回重置前时候的值
    /// </summary>
    /// <returns>重置前的值</returns>
    public int ResetConnectErrorCount()
    {
        return Interlocked.Exchange(ref _connectErrorCount, 0);
    }

    /// <summary>
    /// 自增当前的连续错误计数，并且获取自增后的值信息，最大到10亿为止，无法继续增加了。
    /// </summary>
    /// <returns>自增后的值信息</returns>
    protected int IncrConnectErrorCount()
    {
        var num = Interlocked.Increment(ref _connectErrorCount);
        if (num > 1000000000)
        {
            Interlocked.Exchange(ref _connectErrorCount, 1000000000);
        }
        return num;
    }

    /// <summary>
    /// 主动引发一个管道错误，从而让管道可以重新打开。
    /// </summary>
    public void RaisePipeError()
    {
        Interlocked.CompareExchange(ref _connectErrorCount, 1, 0);
    }

    /// <summary>
    /// 当前的管道连接对象是否发生了错误。
    /// </summary>
    /// <returns>是否发生了通道的异常</returns>
    public virtual bool IsConnectError()
    {
        return _connectErrorCount > 0;
    }

    /// <summary>
    /// 开始后台接收相关的报文数据，当 <see cref="UseServerActivePush" /> 为True时，则使用本方法。
    /// </summary>
    public virtual OperateResult StartReceiveBackground(INetMessage netMessage)
    {
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 设置当前的问答状态下的缓存数据。
    /// </summary>
    /// <param name="buffer">设置的缓存</param>
    protected void SetBufferQA(byte[] buffer)
    {
        BufferQA = buffer;
        _autoResetEvent.Set();
    }

    /// <summary>
    /// 发送数据到当前的管道中去。
    /// </summary>
    /// <param name="data">等待发送的数据</param>
    /// <returns>是否发送成功</returns>
    public async Task<OperateResult> SendAsync(byte[] data)
    {
        if (data == null)
        {
            return OperateResult.CreateSuccessResult();
        }
        return await SendAsync(data, 0, data.Length).ConfigureAwait(false);
    }

    /// <summary>
    /// 将一个数据缓存中的指定的部分字段，发送到当前的管道中去。
    /// </summary>
    /// <param name="data">等待发送的缓存数据</param>
    /// <param name="offset">起始偏移的地址</param>
    /// <param name="size">发送的字节长度信息</param>
    /// <returns>是否发送成功</returns>
    public virtual Task<OperateResult> SendAsync(byte[] data, int offset, int size)
    {
        return Task.FromResult(new OperateResult(StringResources.Language.NotSupportedFunction));
    }

    /// <summary>
    /// 从管道里，接收指定长度的报文数据信息，如果长度指定为-1，表示接收不超过2048字节的动态长度。另外可以指定超时时间，进度报告等。
    /// </summary>
    /// <param name="length">接收的长度信息</param>
    /// <param name="timeOut">指定的超时时间</param>
    /// <returns>是否接收成功的结果对象</returns>
    public virtual async Task<OperateResult<byte[]>> ReceiveAsync(int length, int timeOut)
    {
        if (length == 0)
        {
            return OperateResult.CreateSuccessResult(Array.Empty<byte>());
        }

        var buffer = NetSupport.CreateReceiveBuffer(length);
        if (!buffer.IsSuccess)
        {
            return buffer;
        }
        var receive = await ReceiveAsync(buffer.Content, 0, length, timeOut).ConfigureAwait(false);
        if (!receive.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(receive);
        }
        return OperateResult.CreateSuccessResult(length > 0 ? buffer.Content : buffer.Content.SelectBegin(receive.Content));
    }

    /// <summary>
    /// 打开当前的管道信息，返回是否成功打开的结果对象，并通过属性 Content 指示当前是否为新创建的连接对象，如果是，则该值为 true。
    /// </summary>
    /// <remarks>
    /// 并切换长连接操作
    /// </remarks>
    /// <returns>是否打开成功的结果对象</returns>
    public virtual Task<OperateResult<bool>> OpenCommunicationAsync()
    {
        return Task.FromResult(new OperateResult<bool>(StringResources.Language.NotSupportedFunction));
    }

    /// <summary>
    /// 关闭当前的管道信息，返回是否关闭成功的结果对象。
    /// </summary>
    /// <returns>是否关闭成功</returns>
    public virtual OperateResult CloseCommunication()
    {
        return new OperateResult(StringResources.Language.NotSupportedFunction);
    }

    /// <summary>
    /// 关闭当前的管道信息，返回是否关闭成功的结果对象。
    /// </summary>
    /// <returns>是否关闭成功</returns>
    public virtual Task<OperateResult> CloseCommunicationAsync()
    {
        return Task.FromResult(new OperateResult(StringResources.Language.NotSupportedFunction));
    }

    private async Task<OperateResult<byte[]>> ReceiveCommandLineFromPipeAsync(byte endCode, int timeout = 60000)
    {
        try
        {
            var bufferArray = new List<byte>(128);
            var st = DateTime.Now;
            var bOK = false;
            while ((DateTime.Now - st).TotalMilliseconds < timeout)
            {
                var headResult = await ReceiveAsync(1, timeout).ConfigureAwait(continueOnCapturedContext: false);
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
            if (!bOK)
            {
                return new OperateResult<byte[]>(StringResources.Language.ReceiveDataTimeout + " " + timeout);
            }
            return OperateResult.CreateSuccessResult(bufferArray.ToArray());
        }
        catch (Exception ex2)
        {
            var ex = ex2;
            return new OperateResult<byte[]>(ex.Message);
        }
    }

    public virtual async Task<OperateResult<byte[]>> ReceiveMessageAsync(INetMessage netMessage, byte[] sendValue, bool useActivePush = true)
    {
        if (_useServerActivePush && useActivePush)
        {
            if (_autoResetEvent.WaitOne(ReceiveTimeOut))
            {
                if (netMessage != null)
                {
                    netMessage.HeadBytes = BufferQA;
                }
                return OperateResult.CreateSuccessResult(BufferQA);
            }
            await CloseCommunicationAsync().ConfigureAwait(false);
            return new OperateResult<byte[]>(-IncrConnectErrorCount(), StringResources.Language.ReceiveDataTimeout + ReceiveTimeOut);
        }

        OperateResult<byte[]> read;
        if (netMessage == null || netMessage.ProtocolHeadBytesLength == -1)
        {
            if (netMessage != null && netMessage.SendBytes == null)
            {
                netMessage.SendBytes = sendValue;
            }
            var startTime = DateTime.Now;
            var ms = new MemoryStream();
            do
            {
                read = await ReceiveByMessageAsync(ReceiveTimeOut, null).ConfigureAwait(false);
                if (!read.IsSuccess)
                {
                    return read;
                }
                if (read.Content != null && read.Content.Length != 0)
                {
                    ms.Write(read.Content);
                }
                if (netMessage == null)
                {
                    return OperateResult.CreateSuccessResult(ms.ToArray());
                }
                if (netMessage.CheckReceiveDataComplete(sendValue, ms))
                {
                    return OperateResult.CreateSuccessResult(ms.ToArray());
                }
            }
            while (ReceiveTimeOut < 0 || !((DateTime.Now - startTime).TotalMilliseconds > ReceiveTimeOut));
            return new OperateResult<byte[]>(StringResources.Language.ReceiveDataTimeout + ReceiveTimeOut + " Received: " + ms.ToArray().ToHexString(' '));
        }
        read = await ReceiveByMessageAsync(ReceiveTimeOut, netMessage).ConfigureAwait(false);
        return read;
    }

    protected async Task<OperateResult<byte[]>> ReadFromCoreServerHelperAsync(INetMessage netMessage, byte[] sendValue, bool hasResponseData, int sleep)
    {
        if (netMessage != null)
        {
            netMessage.SendBytes = sendValue;
        }
        var sendResult = await SendAsync(sendValue).ConfigureAwait(continueOnCapturedContext: false);
        if (!sendResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(sendResult);
        }
        if (ReceiveTimeOut < 0)
        {
            return OperateResult.CreateSuccessResult(Array.Empty<byte>());
        }
        if (!hasResponseData)
        {
            return OperateResult.CreateSuccessResult(Array.Empty<byte>());
        }
        if (sleep > 0)
        {
            await Task.Delay(sleep).ConfigureAwait(false);
        }

        var start = DateTime.Now;
        var times = 0;
        OperateResult<byte[]> resultReceive;
        while (true)
        {
            resultReceive = await ReceiveMessageAsync(netMessage, sendValue, true).ConfigureAwait(false);
            if (!resultReceive.IsSuccess)
            {
                return resultReceive;
            }

            bool num;
            if (netMessage != null)
            {
                switch (netMessage.CheckMessageMatch(sendValue, resultReceive.Content!))
                {
                    case 0:
                        return new OperateResult<byte[]>("INetMessage.CheckMessageMatch failed" + Environment.NewLine + StringResources.Language.Send + ": " + SoftBasic.ByteToHexString(sendValue, ' ') + Environment.NewLine + StringResources.Language.Receive + ": " + SoftBasic.ByteToHexString(resultReceive.Content, ' '));
                    case 1:
                        break;
                    default:
                        times++;
                        num = ReceiveTimeOut >= 0 && (DateTime.Now - start).TotalMilliseconds > ReceiveTimeOut;
                        if (num)
                        {
                            return new OperateResult<byte[]>("Receive Message timeout: " + ReceiveTimeOut + " CheckMessageMatch times:" + times);
                        }
                        else
                        {
                            continue;
                        }
                }
            }
            break;
        }
        if (netMessage != null && !netMessage.CheckHeadBytesLegal(null))
        {
            return new OperateResult<byte[]>(StringResources.Language.CommandHeadCodeCheckFailed + Environment.NewLine + StringResources.Language.Send + ": " + SoftBasic.ByteToHexString(sendValue, ' ') + Environment.NewLine + StringResources.Language.Receive + ": " + SoftBasic.ByteToHexString(resultReceive.Content, ' '));
        }
        return OperateResult.CreateSuccessResult(resultReceive.Content);
    }

    private async Task<OperateResult<byte[]>> ReceiveCommandLineFromPipeAsync(byte endCode1, byte endCode2, int timeout = 60000)
    {
        try
        {
            var bufferArray = new List<byte>(128);
            var st = DateTime.Now;
            var bOK = false;
            while ((DateTime.Now - st).TotalMilliseconds < timeout)
            {
                var headResult = await ReceiveAsync(1, timeout).ConfigureAwait(false);
                if (!headResult.IsSuccess)
                {
                    return headResult;
                }

                bufferArray.AddRange(headResult.Content!);
                if (headResult.Content![0] == endCode2 && bufferArray.Count > 1 && bufferArray[^2] == endCode1)
                {
                    bOK = true;
                    break;
                }
            }
            if (!bOK)
            {
                return new OperateResult<byte[]>(StringResources.Language.ReceiveDataTimeout + " " + timeout);
            }
            return OperateResult.CreateSuccessResult(bufferArray.ToArray());
        }
        catch (Exception ex2)
        {
            var ex = ex2;
            return new OperateResult<byte[]>(ex.Message);
        }
    }

    /// <summary>
    /// 接收一条完整的 <seealso cref="INetMessage" /> 数据内容，需要指定超时时间，单位为毫秒。
    /// </summary>
    /// <param name="timeOut">超时时间，单位：毫秒</param>
    /// <param name="netMessage">消息的格式定义</param>
    /// <returns>带有是否成功的byte数组对象</returns>
    private async Task<OperateResult<byte[]>> ReceiveByMessageAsync(int timeOut, INetMessage? netMessage)
    {
        if (netMessage == null)
        {
            return await ReceiveAsync(-1, timeOut).ConfigureAwait(false);
        }

        if (netMessage.ProtocolHeadBytesLength < 0)
        {
            var headCode = BitConverter.GetBytes(netMessage.ProtocolHeadBytesLength);
            var codeLength = headCode[3] & 0xF;
            var receive = codeLength switch
            {
                1 => await ReceiveCommandLineFromPipeAsync(headCode[1], timeOut).ConfigureAwait(false),
                2 => await ReceiveCommandLineFromPipeAsync(headCode[1], headCode[0], timeOut).ConfigureAwait(false),
                _ => null,
            };
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
                var endResult = await ReceiveAsync(message.EndLength, timeOut).ConfigureAwait(false);
                if (!endResult.IsSuccess)
                {
                    return endResult;
                }
                return OperateResult.CreateSuccessResult(SoftBasic.SpliceArray(receive.Content, endResult.Content));
            }
            return receive;
        }

        var headResult = await ReceiveAsync(netMessage.ProtocolHeadBytesLength, timeOut).ConfigureAwait(false);
        if (!headResult.IsSuccess)
        {
            return headResult;
        }

        var start = netMessage.PependedUselesByteLength(headResult.Content!);
        var cycleCount = 0;
        while (start >= netMessage.ProtocolHeadBytesLength)
        {
            headResult = await ReceiveAsync(netMessage.ProtocolHeadBytesLength, timeOut).ConfigureAwait(false);
            if (!headResult.IsSuccess)
            {
                return headResult;
            }
            start = netMessage.PependedUselesByteLength(headResult.Content!);
            cycleCount++;
            if (cycleCount > 10)
            {
                break;
            }
        }
        if (start > 0)
        {
            var head2Result = await ReceiveAsync(start, timeOut).ConfigureAwait(false);
            if (!head2Result.IsSuccess)
            {
                return head2Result;
            }
            headResult.Content = SoftBasic.SpliceArray(headResult.Content!.RemoveBegin(start), head2Result.Content!);
        }

        netMessage.HeadBytes = headResult.Content!;
        var contentLength = netMessage.GetContentLengthByHeadBytes();
        if (contentLength <= 0)
        {
            return OperateResult.CreateSuccessResult(headResult.Content!);
        }

        var result = new byte[netMessage.HeadBytes.Length + contentLength];
        netMessage.HeadBytes.CopyTo(result, 0);
        OperateResult contentResult = await ReceiveAsync(result, netMessage.HeadBytes.Length, contentLength, timeOut).ConfigureAwait(false);
        if (!contentResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(contentResult);
        }
        return OperateResult.CreateSuccessResult(result);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _autoResetEvent?.Dispose();
            _communicationLock?.Dispose();
        }
    }

    public void Dispose()
    {
        if (!_disposedValue)
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
            _disposedValue = true;
        }
    }
}
