using System.IO.Ports;
using ThingsEdge.Communication.Core.IMessage;

namespace ThingsEdge.Communication.Core.Pipe;

/// <summary>
/// 串口管道信息
/// </summary>
public class PipeSerialPort : NetworkPipeBase, IDisposable
{
    private readonly SerialPort _serialPort;

    /// <summary>
    /// 获取或设置一个值，该值指示在串行通信中是否启用请求发送 (RTS) 信号。
    /// </summary>
    public bool RtsEnable
    {
        get => _serialPort.RtsEnable;
        set => _serialPort.RtsEnable = value;
    }

    /// <summary>
    /// 获取或设置一个值，该值指示在串行通信中是否启用数据终端就绪 (Drt) 信号。
    /// </summary>
    public bool DtrEnable
    {
        get => _serialPort.DtrEnable;
        set => _serialPort.DtrEnable = value;
    }

    /// <summary>
    /// 从串口中至少接收的字节长度信息，默认为1个字节
    /// </summary>
    public int AtLeastReceiveLength { get; set; } = 1;

    /// <summary>
    /// 获取或设置连续接收空的数据次数，在数据接收完成时有效，每个单位消耗的时间为 DelayTime。
    /// </summary>
    public int ReceiveEmptyDataCount { get; set; } = 1;

    /// <summary>
    /// 是否在发送数据前清空缓冲数据，默认是false。
    /// </summary>
    public bool IsClearCacheBeforeRead { get; set; }

    /// <summary>
    /// 获取或设置在正式接收对方返回数据前的时候，需要延迟的时间，当设置为0的时候，不需要延迟。
    /// </summary>
    public int DelayTime { get; set; }

    /// <summary>
    /// 实例化一个默认的对象。
    /// </summary>
    public PipeSerialPort()
    {
        _serialPort = new SerialPort();
        DelayTime = 20;
    }

    /// <summary>
    /// 实例化一个默认的对象。
    /// </summary>
    /// <param name="portName">
    /// portName 支持格式化的方式，例如输入 COM3-9600-8-N-1，COM5-19200-7-E-2，其中奇偶校验的字母可选，N:无校验，O：奇校验，E:偶校验，停止位可选 0, 1, 2, 1.5 四种选项。
    /// </param>
    public PipeSerialPort(string portName)
    {
        _serialPort = new SerialPort();
        DelayTime = 20;
        SerialPortInni(portName);
    }

    /// <summary>
    /// 初始化串口信息，9600波特率，8位数据位，1位停止位，无奇偶校验。
    /// </summary>
    /// <remarks>
    /// portName 支持格式化的方式，例如输入 COM3-9600-8-N-1，COM5-19200-7-E-2，其中奇偶校验的字母可选，N:无校验，O：奇校验，E:偶校验，停止位可选 0, 1, 2, 1.5 四种选项。
    /// </remarks>
    /// <param name="portName">端口号信息，例如"COM3"</param>
    public void SerialPortInni(string portName)
    {
        if (portName.Contains('-') || portName.Contains(';'))
        {
            SerialPortInni(delegate (SerialPort sp)
            {
                sp.IniSerialByFormatString(portName);
            });
        }
        else
        {
            SerialPortInni(portName, 9600, 8, StopBits.One, Parity.None);
        }
    }

    /// <summary>
    /// 初始化串口信息，波特率，数据位，停止位，奇偶校验需要全部自己来指定。
    /// </summary>
    /// <param name="portName">端口号信息，例如"COM3"</param>
    /// <param name="baudRate">波特率</param>
    /// <param name="dataBits">数据位</param>
    /// <param name="stopBits">停止位</param>
    /// <param name="parity">奇偶校验</param>
    public void SerialPortInni(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
    {
        if (!_serialPort.IsOpen)
        {
            _serialPort.PortName = portName;
            _serialPort.BaudRate = baudRate;
            _serialPort.DataBits = dataBits;
            _serialPort.StopBits = stopBits;
            _serialPort.Parity = parity;
        }
    }

    /// <summary>
    /// 根据自定义初始化方法进行初始化串口信息。
    /// </summary>
    /// <param name="initi">初始化的委托方法</param>
    public void SerialPortInni(Action<SerialPort> initi)
    {
        if (!_serialPort.IsOpen)
        {
            _serialPort.PortName = "COM1";
            initi(_serialPort);
        }
    }

    /// <summary>
    /// 获取一个值，指示串口是否处于打开状态。
    /// </summary>
    /// <returns>是或否</returns>
    public bool IsOpen()
    {
        return _serialPort.IsOpen;
    }

    /// <summary>
    /// 获取当前的串口对象信息。
    /// </summary>
    /// <returns>串口对象</returns>
    public SerialPort GetPipe()
    {
        return _serialPort;
    }

    public override async Task<OperateResult<bool>> OpenCommunicationAsync()
    {
        await Task.CompletedTask.ConfigureAwait(false);

        try
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
                return OperateResult.CreateSuccessResult(true);
            }
            return OperateResult.CreateSuccessResult(false);
        }
        catch (Exception ex)
        {
            return new OperateResult<bool>((int)CommErrorCode.OpenSerialPortException, "OpenCommunication failed: " + ex.Message);
        }
    }

    public override OperateResult CloseCommunication()
    {
        if (_serialPort.IsOpen)
        {
            try
            {
                _serialPort.Close();
            }
            catch (Exception ex)
            {
                return new OperateResult(ex.Message);
            }
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 清除串口缓冲区的数据，并返回该数据，如果缓冲区没有数据，返回的字节数组长度为0。
    /// </summary>
    /// <returns>是否操作成功的方法</returns>
    private Task<OperateResult<byte[]>> ClearSerialCacheAsync()
    {
        return SerialPortReceivedAsync(_serialPort, null, null, awaitData: false);
    }

    public override async Task<OperateResult> SendAsync(byte[] data)
    {
        await Task.CompletedTask.ConfigureAwait(false);

        if (data.Length != 0)
        {
            try
            {
                _serialPort.Write(data, 0, data.Length);
                return OperateResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return new OperateResult((int)CommErrorCode.SerialPortSendException, ex.Message);
            }
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 从串口接收一串字节数据信息，直到没有数据为止，如果参数awaitData为false, 第一轮接收没有数据则返回。
    /// </summary>
    /// <param name="serialPort">串口对象</param>
    /// <param name="netMessage">定义的消息体对象</param>
    /// <param name="sendValue">等待发送的数据对象</param>
    /// <param name="awaitData">是否必须要等待数据返回</param>
    /// <returns>结果数据对象</returns>
    private async Task<OperateResult<byte[]>> SerialPortReceivedAsync(SerialPort serialPort, INetMessage? netMessage, byte[] sendValue, bool awaitData)
    {
        // TODO: 此代码需要优化
        byte[] array;
        MemoryStream memoryStream;
        try
        {
            array = new byte[1024];
            memoryStream = new MemoryStream();
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>(ex.Message);
        }

        var now = DateTime.Now;
        var num = 0;
        var num2 = 0;
        while (true)
        {
            num2++;
            if (num2 > 1 && DelayTime >= 0)
            {
                await Task.Delay(DelayTime).ConfigureAwait(false);
            }

            try
            {
                if (serialPort.BytesToRead < 1)
                {
                    if (num2 == 1)
                    {
                        continue;
                    }
                    if ((DateTime.Now - now).TotalMilliseconds > ReceiveTimeout)
                    {
                        return new OperateResult<byte[]>((int)CommErrorCode.SerialPortReceiveException, $"Time out: {ReceiveTimeout}, received: {memoryStream.ToArray().ToHexString(' ')}");
                    }
                    if (memoryStream.Length >= AtLeastReceiveLength)
                    {
                        num++;
                        if (netMessage != null || num < ReceiveEmptyDataCount)
                        {
                            continue;
                        }
                    }
                    else if (awaitData)
                    {
                        continue;
                    }
                    break;
                }

                num = 0;
                var num3 = serialPort.Read(array, 0, array.Length);
                if (num3 > 0)
                {
                    memoryStream.Write(array, 0, num3);
                }
                if (netMessage != null && CheckMessageComplete(netMessage, sendValue, ref memoryStream))
                {
                    break;
                }
                if (ReceiveTimeout > 0 && (DateTime.Now - now).TotalMilliseconds > ReceiveTimeout)
                {
                    return new OperateResult<byte[]>((int)CommErrorCode.SerialPortReceiveException, $"Time out: {ReceiveTimeout}, received: {memoryStream.ToArray().ToHexString(' ')}");
                }
                continue;
            }
            catch (Exception ex2)
            {
                return new OperateResult<byte[]>((int)CommErrorCode.SerialPortReceiveException, ex2.Message);
            }
        }
        return OperateResult.CreateSuccessResult(memoryStream.ToArray());
    }

    public override async Task<OperateResult<byte[]>> ReceiveMessageAsync(INetMessage? netMessage, byte[] sendValue, bool useActivePush = true)
    {
        return await SerialPortReceivedAsync(_serialPort, netMessage, sendValue, awaitData: true).ConfigureAwait(continueOnCapturedContext: false);
    }

    public override async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(INetMessage? netMessage, byte[] sendValue, bool hasResponseData)
    {
        if (IsClearCacheBeforeRead)
        {
            await ClearSerialCacheAsync().ConfigureAwait(false);
        }
        return await ReadFromCoreServerHelperAsync(netMessage, sendValue, hasResponseData).ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _serialPort?.Dispose();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "PipeSerialPort[" + _serialPort.ToFormatString() + "]";
    }
}
