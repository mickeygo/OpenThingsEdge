using Nito.AsyncEx;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Profinet.Siemens.Helper;

namespace ThingsEdge.Communication.Profinet.Siemens;

/// <summary>
/// 西门子的PPI协议，适用于s7-200plc，注意，由于本类库的每次通讯分成2次操作，内部增加了一个同步锁，所以单次通信时间比较久，另外，地址支持携带站号，例如：s=2;M100。
/// </summary>
/// <remarks>
/// 适用于西门子200的通信。注意：M地址范围有限 0-31地址。
/// </remarks>
public class SiemensPPI : DeviceSerialPort, ISiemensPPI, IReadWriteNet
{
    private readonly AsyncLock _mutex = new();

    /// <summary>
    /// 西门子PLC的站号信息。
    /// </summary>
    public byte Station { get; set; } = 2;

    /// <summary>
    /// 实例化一个西门子的PPI协议对象。
    /// </summary>
    public SiemensPPI()
    {
        ByteTransform = new ReverseBytesTransform();
        WordLength = 2;
        ReceiveEmptyDataCount = 5;
    }

    protected override INetMessage GetNewNetMessage()
    {
        return new SiemensPPIMessage();
    }

    /// <summary>
    /// 向西门子的PLC中写入byte数据，地址为"M100","AI100","I0","Q0","V100","S100"等。
    /// </summary>
    /// <param name="address">西门子的地址数据信息</param>
    /// <param name="value">数据长度</param>
    /// <returns>带返回结果的结果对象</returns>
    public Task<OperateResult> WriteAsync(string address, byte value)
    {
        return WriteAsync(address, [value]);
    }

    public override Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return SiemensPPIHelper.ReadAsync(this, address, length, Station, _mutex);
    }

    public override Task<OperateResult<bool>> ReadBoolAsync(string address)
    {
        return SiemensPPIHelper.ReadBoolAsync(this, address, Station, _mutex);
    }

    public override Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return SiemensPPIHelper.ReadBoolAsync(this, address, length, Station, _mutex);
    }

    public override Task<OperateResult> WriteAsync(string address, byte[] values)
    {
        return SiemensPPIHelper.WriteAsync(this, address, values, Station, _mutex);
    }

    public override Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return SiemensPPIHelper.WriteAsync(this, address, values, Station, _mutex);
    }
  
    public Task<OperateResult> StartAsync(string parameter = "")
    {
        return SiemensPPIHelper.StartAsync(this, parameter, Station, _mutex);
    }

    public Task<OperateResult> StopAsync(string parameter = "")
    {
        return SiemensPPIHelper.StopAsync(this, parameter, Station, _mutex);
    }

    public Task<OperateResult<string>> ReadPlcTypeAsync(string parameter = "")
    {
        return SiemensPPIHelper.ReadPlcTypeAsync(this, parameter, Station, _mutex);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"SiemensPPI[{PortName}:{BaudRate}]";
    }
}
