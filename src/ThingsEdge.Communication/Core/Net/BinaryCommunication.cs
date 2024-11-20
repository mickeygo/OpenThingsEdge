using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Core.Pipe;

namespace ThingsEdge.Communication.Core.Net;

/// <summary>
/// 基于二进制的通信基础类。
/// </summary>
public abstract class BinaryCommunication
{
    /// <summary>
    /// 当前连接的唯一ID号，默认为长度20的guid码加随机数组成，方便列表管理。
    /// </summary>
    public string ConnectionId { get; }

    /// <summary>
    /// 获取当前的管道信息，管道类型为 <see cref="PipeNetBase" /> 的继承类，
    /// 内置 <see cref="PipeTcpNet" /> 管道和 <see cref="PipeSerialPort" /> 管道。
    /// </summary>
    [NotNull]
    public PipeNetBase? Pipe { get; init; }

    /// <summary>
    /// 组件的日志工具。
    /// </summary>
    public ILogger? Logger { get; set; }

    /// <summary>
    /// 获取或设置接收服务器反馈的时间，如果为负数，则不接收反馈。
    /// </summary>
    public int ReceiveTimeOut
    {
        get => Pipe.ReceiveTimeout;
        set => Pipe.ReceiveTimeout = value;
    }

    /// <summary>
    /// 获取或设置在正式接收对方返回数据前的时候，需要休息的时间，当设置为0的时候，不需要休息。
    /// </summary>
    public int SleepTime
    {
        get => Pipe.DelayTime;
        set => Pipe.DelayTime = value;
    }

    /// <summary>
    /// 默认的无参构造函数。
    /// </summary>
    public BinaryCommunication()
    {
        ConnectionId = SoftBasic.GetUniqueStringByGuidAndRandom();
    }

    /// <summary>
    /// 获取一个新的消息对象的方法，需要在继承类里面进行重写。
    /// </summary>
    /// <returns>消息类对象</returns>
    protected virtual INetMessage GetNewNetMessage()
    {
        return default;
    }

    /// <summary>
    /// 根据实际的协议选择是否重写本方法，有些协议在断开连接之前，需要发送一些报文来关闭当前的网络通道。
    /// </summary>
    /// <returns>当断开连接时额外的操作结果</returns>
    protected virtual OperateResult ExtraOnDisconnect()
    {
        return OperateResult.CreateSuccessResult();
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
    /// <returns>是否初始化成功，依据具体的协议进行重写</returns>
    protected virtual async Task<OperateResult> InitializationOnConnectAsync()
    {
        return await Task.FromResult(OperateResult.CreateSuccessResult()).ConfigureAwait(false);
    }

    /// <summary>
    /// 根据实际的协议选择是否重写本方法，有些协议在断开连接之前，需要发送一些报文来关闭当前的网络通道。
    /// </summary>
    /// <returns>当断开连接时额外的操作结果</returns>
    protected virtual async Task<OperateResult> ExtraOnDisconnectAsync()
    {
        return await Task.FromResult(OperateResult.CreateSuccessResult()).ConfigureAwait(false);
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
    /// 将二进制的数据发送到管道中去，然后从管道里接收二进制的数据回来，并返回是否成功的结果对象。
    /// </summary>
    /// <param name="send">发送的完整的报文信息</param>
    /// <returns>接收的完整的报文信息</returns>
    public virtual async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(byte[] send)
    {
        return await ReadFromCoreServerAsync(send, hasResponseData: true, usePackAndUnpack: true).ConfigureAwait(false);
    }

    /// <summary>
    /// 根据对方返回的报文命令，对命令进行基本的拆包，例如各种Modbus协议拆包为统一的核心报文，还支持对报文的验证。
    /// </summary>
    /// <remarks>
    /// 在实际解包的操作过程中，通常对状态码，错误码等消息进行判断，如果校验不通过，将携带错误消息返回。
    /// </remarks>
    /// <param name="sends">发送的原始报文数据</param>
    /// <returns>返回拆包之后的报文信息，默认不进行任何的拆包操作</returns>
    public virtual async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(IEnumerable<byte[]> sends)
    {
        return await NetSupport.ReadFromCoreServerAsync(sends, ReadFromCoreServerAsync).ConfigureAwait(false);
    }

    /// <summary>
    /// 将二进制的数据发送到管道中去，然后从管道里接收二进制的数据回来，并返回是否成功的结果对象。
    /// </summary>
    /// <param name="send">发送的完整的报文信息</param>
    /// <param name="hasResponseData">是否有等待的数据返回</param>
    /// <param name="usePackAndUnpack">是否需要对命令重新打包，在重写<see cref="PackCommandWithHeader(byte[])" />方法后才会有影响</param>
    /// <returns>接收的完整的报文信息</returns>
    public virtual async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(byte[] send, bool hasResponseData, bool usePackAndUnpack)
    {
        var enter = Pipe.CommunicationLock.EnterLock(Pipe.ReceiveTimeout);
        if (!enter.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(enter);
        }

        var read = new OperateResult<byte[]>();
        try
        {
            var pipe = await Pipe.OpenCommunicationAsync().ConfigureAwait(false);
            if (!pipe.IsSuccess)
            {
                read.CopyErrorFromOther(pipe);
                return read;
            }
            if (pipe.Content)
            {
                var ini = await InitializationOnConnectAsync().ConfigureAwait(false);
                if (!ini.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<byte[]>(ini);
                }
            }
            read = await ReadFromCoreServerAsync(Pipe, send, hasResponseData, usePackAndUnpack).ConfigureAwait(false);
            ExtraAfterReadFromCoreServer(read);
            Pipe.CommunicationLock.LeaveLock();
        }
        catch
        {
            throw;
        }
        finally
        {
            Pipe.CommunicationLock.LeaveLock();
        }

        if (!Pipe.IsPersistentConnection)
        {
            await ExtraOnDisconnectAsync().ConfigureAwait(false);
            await Pipe.CloseCommunicationAsync().ConfigureAwait(false);
        }
        return read;
    }

    /// <summary>
    /// 使用指定的管道来进行数据通信，发送原始数据到管道，然后从管道接收相关的数据返回，本方法无锁。
    /// </summary>
    /// <param name="pipe">管道信息</param>
    /// <param name="send">发送的完整的报文信息</param>
    /// <param name="hasResponseData">是否有等待的数据返回</param>
    /// <param name="usePackAndUnpack">是否需要对命令重新打包，在重写 <see cref="PackCommandWithHeader" /> 方法后才会有影响</param>
    /// <returns>是否成功的结果对象</returns>
    public virtual async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(PipeNetBase pipe, byte[] send, bool hasResponseData, bool usePackAndUnpack)
    {
        if (usePackAndUnpack)
        {
            send = PackCommandWithHeader(send);
        }

        var read = await pipe.ReadFromCoreServerAsync(GetNewNetMessage(), send, hasResponseData).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        if (!usePackAndUnpack)
        {
            return read;
        }
        var unpack = UnpackResponseContent(send, read.Content);
        if (!unpack.IsSuccess && unpack.ErrorCode == int.MinValue)
        {
            unpack.ErrorCode = 10000;
        }
        return unpack;
    }
}
