using System.Net.Sockets;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Core.Net;
using ThingsEdge.Communication.Secs.Helper;
using ThingsEdge.Communication.Secs.Message;
using ThingsEdge.Communication.Secs.Types;

namespace ThingsEdge.Communication.Secs;

/// <summary>
/// HSMS的协议实现，SECS基于TCP的版本。
/// </summary>
/// <remarks>
/// </remarks>
public class SecsHsms : NetworkDoubleBase, ISecs
{
    /// <summary>
    /// Secs消息接收的事件
    /// </summary>
    /// <param name="sender">数据的发送方</param>
    /// <param name="secsMessage">消息内容</param>
    public delegate void OnSecsMessageReceivedDelegate(object sender, SecsMessage secsMessage);

    private readonly SoftIncrementCount _incrementCount;

    private readonly List<uint> _identityQAs = [];

    /// <summary>
    /// 获取或设置当前的DeivceID信息
    /// </summary>
    public ushort DeviceID { get; set; }

    /// <summary>
    /// 获取或设置当前的GEM信息，可以用来方便的调用一些常用的功能接口，或是自己实现自定义的接口方法
    /// </summary>
    public Gem Gem { get; set; }

    /// <summary>
    /// 是否使用S0F0来初始化当前的设备对象信息
    /// </summary>
    public bool InitializationS0F0 { get; set; }

    /// <summary>
    /// 获取或设置用于字符串解析的编码信息
    /// </summary>
    public Encoding StringEncoding { get; set; } = Encoding.Default;

    /// <summary>
    /// 当接收到非应答消息的时候触发的事件
    /// </summary>
    public event OnSecsMessageReceivedDelegate OnSecsMessageReceived;

    /// <summary>
    /// 实例化一个默认的对象<br />
    /// instantiate a default object
    /// </summary>
    public SecsHsms()
    {
        _incrementCount = new SoftIncrementCount(4294967295L, 1L);
        ByteTransform = new ReverseBytesTransform();
        UseServerActivePush = true;
        Gem = new Gem(this);
    }

    /// <summary>
    /// 指定ip地址和端口号来实例化一个默认的对象。
    /// </summary>
    /// <param name="ipAddress">PLC的Ip地址</param>
    /// <param name="port">PLC的端口</param>
    public SecsHsms(string ipAddress, int port)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new SecsHsmsMessage();
    }

    /// <inheritdoc />
    protected override async Task<OperateResult> InitializationOnConnectAsync(Socket socket)
    {
        if (InitializationS0F0)
        {
            await SendAsync(socket, Secs1.BuildHSMSMessage(ushort.MaxValue, 0, 0, 1, (uint)_incrementCount.GetCurrentValue(), null, wBit: false)).ConfigureAwait(false);
        }
        return await base.InitializationOnConnectAsync(socket).ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override bool DecideWhetherQAMessage(Socket socket, OperateResult<byte[]> receive)
    {
        if (!receive.IsSuccess)
        {
            return false;
        }

        var content = receive.Content;
        SecsMessage secsMessage;
        try
        {
            secsMessage = new SecsMessage(content, 4);
        }
        catch (Exception ex)
        {
            Logger?.WriteException(ToString(), "DecideWhetherQAMessage.SecsMessage.cor", ex);
            return false;
        }

        secsMessage.StringEncoding = StringEncoding;
        if (secsMessage.StreamNo == 0 && secsMessage.FunctionNo == 0 && secsMessage.BlockNo % 2 == 1)
        {
            Send(socket, Secs1.BuildHSMSMessage(ushort.MaxValue, 0, 0, (ushort)(secsMessage.BlockNo + 1), secsMessage.MessageID, null, wBit: false));
            return false;
        }
        if (secsMessage.FunctionNo % 2 == 0 && secsMessage.FunctionNo != 0)
        {
            var flag = false;
            lock (_identityQAs)
            {
                flag = _identityQAs.Remove(secsMessage.MessageID);
            }
            if (flag)
            {
                return flag;
            }
        }
        if (secsMessage.StreamNo == 1 && secsMessage.FunctionNo == 13)
        {
            SendByCommand(1, 14, new SecsValue(
            [
                new byte[1],
                SecsValue.EmptyListValue()
            ]).ToSourceBytes(), back: false);
            return false;
        }
        if (secsMessage.StreamNo == 2 && secsMessage.FunctionNo == 17)
        {
            SendByCommand(2, 18, new SecsValue(DateTime.Now.ToString("yyyyMMddHHmmssff")), back: false);
            return false;
        }
        if (secsMessage.StreamNo == 1 && secsMessage.FunctionNo == 1)
        {
            SendByCommand(1, 2, SecsValue.EmptyListValue(), back: false);
            return false;
        }
        OnSecsMessageReceived?.Invoke(this, secsMessage);
        return false;
    }

    /// <inheritdoc cref="M:HslCommunication.Secs.Types.ISecs.SendByCommand(System.Byte,System.Byte,System.Byte[],System.Boolean)" />
    public OperateResult SendByCommand(byte stream, byte function, byte[] data, bool back)
    {
        var send = Secs1.BuildHSMSMessage(DeviceID, stream, function, 0, (uint)_incrementCount.GetCurrentValue(), data, back);
        return ReadFromCoreServer(send, hasResponseData: false);
    }

    /// <inheritdoc cref="M:HslCommunication.Secs.Types.ISecs.SendByCommand(System.Byte,System.Byte,HslCommunication.Secs.Types.SecsValue,System.Boolean)" />
    public OperateResult SendByCommand(byte stream, byte function, SecsValue data, bool back)
    {
        return SendByCommand(stream, function, data.ToSourceBytes(StringEncoding), back);
    }

    /// <inheritdoc cref="M:HslCommunication.Secs.Types.ISecs.ReadSecsMessage(System.Byte,System.Byte,System.Byte[],System.Boolean)" />
    public OperateResult<SecsMessage> ReadSecsMessage(byte stream, byte function, byte[] data, bool back)
    {
        var num = (uint)_incrementCount.GetCurrentValue();
        lock (_identityQAs)
        {
            _identityQAs.Add(num);
        }
        var operateResult = ReadFromCoreServer(Secs1.BuildHSMSMessage(DeviceID, stream, function, 0, num, data, back));
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<SecsMessage>(operateResult);
        }
        return OperateResult.CreateSuccessResult(new SecsMessage(operateResult.Content, 4)
        {
            StringEncoding = StringEncoding
        });
    }

    public OperateResult<SecsMessage> ReadSecsMessage(byte stream, byte function, SecsValue data, bool back)
    {
        return ReadSecsMessage(stream, function, data.ToSourceBytes(StringEncoding), back);
    }

    public async Task<OperateResult> SendByCommandAsync(byte stream, byte function, byte[] data, bool back)
    {
        var command = Secs1.BuildHSMSMessage(DeviceID, stream, function, 0, (uint)_incrementCount.GetCurrentValue(), data, back);
        return await ReadFromCoreServerAsync(command, hasResponseData: false).ConfigureAwait(false);
    }

    public async Task<OperateResult> SendByCommandAsync(byte stream, byte function, SecsValue data, bool back)
    {
        return await SendByCommandAsync(stream, function, data.ToSourceBytes(StringEncoding), back).ConfigureAwait(false);
    }

    public async Task<OperateResult<SecsMessage>> ReadSecsMessageAsync(byte stream, byte function, byte[] data, bool back)
    {
        var identityQA = (uint)_incrementCount.GetCurrentValue();
        lock (_identityQAs)
        {
            _identityQAs.Add(identityQA);
        }
        var read = await ReadFromCoreServerAsync(Secs1.BuildHSMSMessage(DeviceID, stream, function, 0, identityQA, data, back)).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<SecsMessage>(read);
        }
        return OperateResult.CreateSuccessResult(new SecsMessage(read.Content!, 4)
        {
            StringEncoding = StringEncoding
        });
    }

    public async Task<OperateResult<SecsMessage>> ReadSecsMessageAsync(byte stream, byte function, SecsValue data, bool back)
    {
        return await ReadSecsMessageAsync(stream, function, data.ToSourceBytes(StringEncoding), back).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"SecsHsms[{IpAddress}:{Port}]";
    }
}
