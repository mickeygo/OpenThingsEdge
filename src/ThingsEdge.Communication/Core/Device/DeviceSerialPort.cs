using System.IO.Ports;
using ThingsEdge.Communication.Core.Pipe;

namespace ThingsEdge.Communication.Core.Device;

/// <summary>
/// 基于串口的设备类对象信息基础类。
/// </summary>
public abstract class DeviceSerialPort : DeviceCommunication
{
    private readonly PipeSerialPort _pipe;

    /// <inheritdoc cref="PipeSerialPort.RtsEnable" />
    public bool RtsEnable
    {
        get => _pipe.RtsEnable;
        set => _pipe.RtsEnable = value;
    }

    /// <inheritdoc cref="PipeSerialPort.ReceiveEmptyDataCount" />
    public int ReceiveEmptyDataCount
    {
        get => _pipe.ReceiveEmptyDataCount;
        set => _pipe.ReceiveEmptyDataCount = value;
    }

    /// <summary>
    /// 是否在发送数据前清空缓冲数据，默认是false。
    /// </summary>
    public bool IsClearCacheBeforeRead
    {
        get => _pipe.IsClearCacheBeforeRead;
        set => _pipe.IsClearCacheBeforeRead = value;
    }

    /// <summary>
    /// 当前连接串口信息的端口号名称。
    /// </summary>
    public string? PortName { get; private set; }

    /// <summary>
    /// 当前连接串口信息的波特率。
    /// </summary>
    public int BaudRate { get; private set; }

    /// <summary>
    /// 实例化一个默认的对象
    /// </summary>
    public DeviceSerialPort()
    {
        NetworkPipe = _pipe = new PipeSerialPort();
    }

    /// <inheritdoc cref="PipeSerialPort.SerialPortInni(string)" />
    public virtual void SerialPortInni(string portName)
    {
        _pipe.SerialPortInni(portName);
    }

    /// <summary>
    /// 初始化串口信息，波特率，8位数据位，1位停止位，无奇偶校验。
    /// </summary>
    /// <param name="portName">端口号信息，例如"COM3"</param>
    /// <param name="baudRate">波特率</param>
    public virtual void SerialPortInni(string portName, int baudRate)
    {
        SerialPortInni(portName, baudRate, 8, StopBits.One, Parity.None);
    }

    /// <inheritdoc cref="PipeSerialPort.SerialPortInni(string,int,int,StopBits,Parity)" />
    public virtual void SerialPortInni(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
    {
        _pipe.SerialPortInni(portName, baudRate, dataBits, stopBits, parity);
        PortName = portName;
        BaudRate = baudRate;
    }

    /// <inheritdoc cref="PipeSerialPort.SerialPortInni(Action{SerialPort})" />
    public void SerialPortInni(Action<SerialPort> initi)
    {
        _pipe.SerialPortInni(initi);
        PortName = _pipe.GetPipe().PortName;
        BaudRate = _pipe.GetPipe().BaudRate;
    }

    /// <summary>
    /// 打开一个新的串行端口连接。
    /// </summary>
    public virtual async Task<OperateResult> OpenAsync()
    {
        var operateResult = await NetworkPipe.CreateAndConnectPipeAsync().ConfigureAwait(false);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        return await InitializationOnConnectAsync().ConfigureAwait(false);
    }

    protected override async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(NetworkPipeBase pipe, byte[] send, bool hasResponseData, bool usePackAndUnpack)
    {
        using (await pipe.Lock.LockAsync(ReceiveTimeout).ConfigureAwait(false))
        {
            return await base.ReadFromCoreServerAsync(pipe, send, hasResponseData, usePackAndUnpack).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 获取一个值，指示串口是否处于打开状态。
    /// </summary>
    /// <returns>是或否</returns>
    public bool IsOpen()
    {
        return _pipe.GetPipe().IsOpen;
    }

    /// <summary>
    /// 关闭当前的串口连接。
    /// </summary>
    public override void Close()
    {
        if (_pipe.GetPipe().IsOpen)
        {
            base.Close();
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"DeviceSerialPort<{ByteTransform}>{{{NetworkPipe}}}";
    }
}
