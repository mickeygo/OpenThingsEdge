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
public class SiemensPPIOverTcp : DeviceTcpNet, ISiemensPPI, IReadWriteNet
{
    public byte Station { get; set; } = 2;

    /// <summary>
    /// 使用指定的ip地址和端口号来实例化对象。
    /// </summary>
    /// <param name="ipAddress">Ip地址信息</param>
    /// <param name="port">端口号信息</param>
    public SiemensPPIOverTcp(string ipAddress, int port) : base(ipAddress, port)
    {
        WordLength = 2;
        ByteTransform = new ReverseBytesTransform();
    }

    protected override INetMessage GetNewNetMessage()
    {
        return new SiemensPPIMessage();
    }

    public override Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return SiemensPPIHelper.ReadAsync(this, address, length, Station, NetworkPipe.Lock);
    }

    public override Task<OperateResult<bool>> ReadBoolAsync(string address)
    {
        return SiemensPPIHelper.ReadBoolAsync(this, address, Station, NetworkPipe.Lock);
    }

    public override Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return SiemensPPIHelper.ReadBoolAsync(this, address, length, Station, NetworkPipe.Lock);
    }

    public async Task<OperateResult<byte>> ReadByteAsync(string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadAsync(address, 1).ConfigureAwait(false));
    }

    public override Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        return SiemensPPIHelper.WriteAsync(this, address, data, Station, NetworkPipe.Lock);
    }

    public Task<OperateResult> WriteAsync(string address, byte value)
    {
        return WriteAsync(address, [value]);
    }

    public override Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return SiemensPPIHelper.WriteAsync(this, address, values, Station, NetworkPipe.Lock);
    }

    public Task<OperateResult> StartAsync(string parameter = "")
    {
        return SiemensPPIHelper.StartAsync(this, parameter, Station, NetworkPipe.Lock);
    }

    public Task<OperateResult> StopAsync(string parameter = "")
    {
        return SiemensPPIHelper.StopAsync(this, parameter, Station, NetworkPipe.Lock);
    }

    public Task<OperateResult<string>> ReadPlcTypeAsync(string parameter = "")
    {
        return SiemensPPIHelper.ReadPlcTypeAsync(this, parameter, Station, NetworkPipe.Lock);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"SiemensPPIOverTcp[{IpAddress}:{Port}]";
    }
}
